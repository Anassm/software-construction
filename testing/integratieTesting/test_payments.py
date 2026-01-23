import pytest
import requests
import uuid
import os

@pytest.fixture
def _data():
    return {
        "base": os.environ["BASE_URL"],
        "users": {
            "user_a": {
                "email": os.environ["USER_A_EMAIL"],
                "password": os.environ["USER_A_PASSWORD"],
                "username": os.environ["USER_A_USERNAME"],
                "name": os.environ["USER_A_NAME"],
                "role": "user"
            },
            "user_b": {
                "email": os.environ["USER_B_EMAIL"],
                "password": os.environ["USER_B_PASSWORD"],
                "username": os.environ["USER_B_USERNAME"],
                "name": os.environ["USER_B_NAME"],
                "role": "user"
            },
            "admin": {
                "email": os.environ["ADMIN_EMAIL"],
                "password": os.environ["ADMIN_PASSWORD"],
                "username": os.environ["ADMIN_USERNAME"],
                "name": os.environ["ADMIN_NAME"],
                "role": "admin"
            }
        }
    }


def register_and_login(base_url, user):
    requests.post(f"{base_url}/register", json=user)
    r = requests.post(
        f"{base_url}/login",
        json={"username": user["username"], "password": user["password"]},
    )
    if r.status_code != 200 or "accesstoken" not in r.json():
        pytest.fail(f"Fout bij inloggen (401): {r.status_code} - {r.text}")
    body = r.json()
    return {"Authorization": f"{body['tokentype']} {body['accesstoken']}"}


@pytest.fixture
def user_token(_data):
    return register_and_login(_data["base"], _data["users"]["user_a"])


@pytest.fixture
def admin_token(_data):
    return register_and_login(_data["base"], _data["users"]["admin"])


@pytest.fixture
def user_headers(user_token):
    return {**user_token, "Content-Type": "application/json"}


@pytest.fixture
def admin_headers(admin_token):
    return {**admin_token, "Content-Type": "application/json"}


def get_v1_post_payload(username):
    return {
        "amount": 50.00,
        "transaction": f"tx_{uuid.uuid4()}",
        "sessionID": None,
    }


@pytest.fixture
def setup_payment(_data, user_headers):
    payload = get_v1_post_payload(_data["users"]["user_a"]["username"])
    response = requests.post(_data["base"] + "/payments", headers=user_headers, json=payload)
    if response.status_code != 201:
        pytest.fail(
            f"Setup payment failed (kan betaling niet aanmaken): {response.status_code} - {response.text}"
        )
    return response.json()["payment"]["id"]


@pytest.fixture
def discount_url(_data):
    return f"{_data['base']}/discounts"


@pytest.fixture
def discount_url(_data):
    return f"{_data['base']}/discounts"


@pytest.fixture
def created_discount_code(_data, discount_url, admin_headers):
    unique_code = f"TEST_{uuid.uuid4().hex[:8].upper()}"

    payload = {
        "code": unique_code,
        "isActive": True,
        "startDate": None,
        "expiryDate": "2099-12-31T23:59:59Z",
        "maxUsage": None,
        "percentage": 0,
        "fixedAmount": 10.00,
        "allowedLocation": None,
    }

    response = requests.post(discount_url, headers=admin_headers, json=payload)

    if response.status_code != 201:
        pytest.fail(
            f"Setup fail: Kon geen test-discount aanmaken. Status: {response.status_code}. Body: {response.text}"
        )

    return unique_code


def test_get_payments_no_auth(_data):
    response = requests.get(_data["base"] + "/payments")
    assert response.status_code == 401
    assert "Unauthorized" in response.json()["error"]


def test_get_payments_invalid_token(_data):
    response = requests.get(
        _data["base"] + "/payments", headers={"Authorization": "Bearer invalid_token"}
    )
    assert response.status_code == 401
    assert "Unauthorized" in response.json()["error"]


def test_get_payments_by_username_admin(_data, admin_headers, user_headers):
    requests.post(
        _data["base"] + "/payments",
        headers=user_headers,
        json=get_v1_post_payload(_data["users"]["user_a"]["username"]),
    )
    response = requests.get(
        f"{_data['base']}/payments/{_data['users']['user_a']['username']}", headers=admin_headers
    )
    assert response.status_code in [200, 403, 404]


def test_get_payments_by_username_user_forbidden(_data, user_headers):
    response = requests.get(f"{_data['base']}/payments/someoneelse", headers=user_headers)
    assert response.status_code == 403


def test_post_payment_no_auth(_data):
    data = get_v1_post_payload("guest")
    response = requests.post(_data["base"] + "/payments", json=data)
    assert response.status_code == 401


def test_post_payment_missing_field(_data, user_headers):
    data = {"transaction": "tx_missing_amount"}
    response = requests.post(_data["base"] + "/payments", headers=user_headers, json=data)
    assert response.status_code == 400
    body = response.json()
    assert "error" in body
    assert "amount" in body["error"]


def test_post_payment_success(_data, user_headers):
    data = get_v1_post_payload(_data["users"]["user_a"]["username"])
    response = requests.post(_data["base"] + "/payments", headers=user_headers, json=data)
    assert response.status_code == 201
    data = response.json()
    assert data["status"] == "Success"
    assert "payment" in data


def test_post_refund_no_auth(_data):
    refund_data = {"paymentId": str(uuid.uuid4()), "reason": "Test"}
    response = requests.post(f"{_data['base']}/payments/refund", json=refund_data)
    assert response.status_code == 401


def test_post_refund_user_forbidden(_data, user_headers):
    refund_data = {"paymentId": str(uuid.uuid4()), "reason": "Test"}
    response = requests.post(
        f"{_data['base']}/payments/refund", headers=user_headers, json=refund_data
    )
    assert response.status_code == 403


# def test_post_refund_admin_success(_data, admin_headers, setup_payment):
#     refund_data = {"paymentId": setup_payment, "reason": "Test Refund"}
#     response = requests.post(
#         f"{_data['url']}/refund", headers=admin_headers, json=refund_data
#     )

#     assert response.status_code == 201
#     data = response.json()
#     assert data["status"] == "Success"
#     assert "payment" in data
#     assert data["payment"]["hash"].startswith("REFUND:")


def test_put_payment_no_auth(_data, setup_payment):
    url = f"{_data['base']}/payments/{setup_payment}"
    response = requests.put(url, json={"t_data": {"info": "ok"}, "validation": "hash"})
    assert response.status_code == 401


def test_put_payment_missing_field(_data, user_headers, setup_payment):
    url = f"{_data['base']}/payments/{setup_payment}"
    data = {"t_data": {"info": "ok"}}
    response = requests.put(url, headers=user_headers, json=data)
    assert response.status_code == 400
    body = response.json()
    assert "errors" in body


def test_put_payment_invalid_hash(_data, user_headers, setup_payment):
    url = f"{_data['base']}/payments/{setup_payment}"
    data = {"t_data": {"info": "ok"}, "validation": "invalid_hash"}
    response = requests.put(url, headers=user_headers, json=data)

    assert response.status_code == 401
    assert "Validation failed" in response.json()["error"]


def test_put_payment_success(_data, user_headers, setup_payment):
    url = f"{_data['base']}/payments/{setup_payment}"
    data = {"t_data": {"info": "complete"}, "validation": "hash123"}
    response = requests.put(url, headers=user_headers, json=data)

    assert response.status_code == 200
    data = response.json()
    assert data["status"] == "Success"
    assert "payment" in data
    assert data["payment"]["hash"] == "hash123"


# Discount codes
def get_post_payload_with_discount(username, discount_code):
    payload = get_v1_post_payload(username)
    payload["discountCode"] = discount_code
    return payload


def test_post_payment_with_valid_discount(_data, user_headers, created_discount_code):
    payload = get_post_payload_with_discount(
        _data["users"]["user_a"]["username"], created_discount_code
    )

    response = requests.post(_data["base"] + "/payments", headers=user_headers, json=payload)

    if response.status_code != 201:
        pytest.fail(
            f"Payment met discount mislukt. Status: {response.status_code}. Response: {response.text}"
        )

    data = response.json()
    assert data["status"] == "Success"

    assert "discount" in data, "Response mist het 'discount' object"

    disc_data = data["discount"]

    original = float(disc_data["originalAmount"])
    discount = float(disc_data["discountAmount"])
    final = float(disc_data["finalAmount"])

    assert original == 50.00
    assert discount == 10.00
    assert final == 40.00

    assert final + discount == pytest.approx(original)


def test_post_payment_with_invalid_discount(_data, user_headers):
    fake_code = f"FAKE_{uuid.uuid4()}"
    payload = get_post_payload_with_discount(
        _data["users"]["user_a"]["username"], fake_code
    )

    response = requests.post(_data["base"] + "/payments", headers=user_headers, json=payload)

    assert response.status_code in [400, 404]

    body = response.json()
    assert "error" in body
