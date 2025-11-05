import pytest
import requests
import uuid


@pytest.fixture
def _data():
    user_uuid = str(uuid.uuid4())[:8]
    admin_uuid = str(uuid.uuid4())[:8]
    return {
        "base": "http://localhost:8000",
        "url": "http://localhost:8000/payments",
        "users": {
            "user_a": {
                "email": f"user_a_{user_uuid}@paymentstest.com",
                "password": "UserPass100!",
                "username": f"paymentuser_a_{user_uuid}",
                "name": "Payment User A",
                "role": "user",
            },
            "admin": {
                "email": f"admin_{admin_uuid}@paymentstest.com",
                "password": "AdminPass100!",
                "username": f"paymentadmin_{admin_uuid}",
                "name": "Payment Admin",
                "role": "admin",
            },
        },
    }


def register_and_login(base_url, user):
    register_payload = {
        "email": user["email"],
        "password": user["password"],
        "username": user["username"],
    }

    reg_response = requests.post(f"{base_url}/register", json=register_payload)
    if reg_response.status_code != 200 and reg_response.status_code != 201:
        pytest.fail(
            f"Fout bij registreren: {reg_response.status_code} - {reg_response.text}"
        )

    login_response = requests.post(
        f"{base_url}/login", json={"email": user["email"], "password": user["password"]}
    )

    if login_response.status_code != 200 or "accessToken" not in login_response.json():
        pytest.fail(
            f"Fout bij inloggen (401): {login_response.status_code} - {login_response.text}"
        )
    return {"Authorization": f"Bearer {login_response.json()['accessToken']}"}


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
        "sessionID": str(uuid.uuid4()),
    }


@pytest.fixture
def setup_payment(_data, user_headers):
    payload = get_v1_post_payload(_data["users"]["user_a"]["username"])
    response = requests.post(_data["url"], headers=user_headers, json=payload)
    if response.status_code != 201:
        pytest.fail(f"Setup payment failed: {response.status_code} - {response.text}")
    return response.json()["payment"]["id"]


def test_get_payments_no_auth(_data):
    response = requests.get(_data["url"])
    assert response.status_code == 401
    assert "Unauthorized" in response.json()["error"]


def test_get_payments_user(_data, user_headers):
    response = requests.get(_data["url"], headers=user_headers)
    assert response.status_code == 200
    assert isinstance(response.json(), list)


def test_get_payments_invalid_token(_data):
    response = requests.get(
        _data["url"], headers={"Authorization": "Bearer invalid_token"}
    )
    assert response.status_code == 401
    assert "Unauthorized" in response.json()["error"]


def test_get_payments_by_username_admin(_data, admin_headers, user_headers):
    requests.post(
        _data["url"],
        headers=user_headers,
        json=get_v1_post_payload(_data["users"]["user_a"]["username"]),
    )
    response = requests.get(
        f"{_data['url']}/{_data['users']['user_a']['username']}", headers=admin_headers
    )
    assert response.status_code == 200
    assert isinstance(response.json(), list)


def test_get_payments_by_username_user_forbidden(_data, user_headers):
    response = requests.get(f"{_data['url']}/someoneelse", headers=user_headers)
    assert response.status_code == 403


def test_post_payment_no_auth(_data):
    data = get_v1_post_payload("guest")
    response = requests.post(_data["url"], json=data)
    assert response.status_code == 401


def test_post_payment_missing_field(_data, user_headers):
    data = {"transaction": "tx_missing_amount"}
    response = requests.post(_data["url"], headers=user_headers, json=data)
    assert response.status_code == 400
    body = response.json()
    assert "error" in body
    assert "amount" in body["error"]


def test_post_payment_success(_data, user_headers):
    data = get_v1_post_payload(_data["users"]["user_a"]["username"])
    response = requests.post(_data["url"], headers=user_headers, json=data)
    assert response.status_code == 201
    data = response.json()
    assert data["status"] == "Success"
    assert "payment" in data


def test_post_refund_no_auth(_data):
    refund_data = {"paymentId": str(uuid.uuid4()), "reason": "Test"}
    response = requests.post(f"{_data['url']}/refund", json=refund_data)
    assert response.status_code == 401


def test_post_refund_user_forbidden(_data, user_headers):
    refund_data = {"paymentId": str(uuid.uuid4()), "reason": "Test"}
    response = requests.post(
        f"{_data['url']}/refund", headers=user_headers, json=refund_data
    )
    assert response.status_code == 403


def test_post_refund_admin_success(_data, admin_headers, setup_payment):
    refund_data = {"paymentId": setup_payment, "reason": "Test Refund"}
    response = requests.post(
        f"{_data['url']}/refund", headers=admin_headers, json=refund_data
    )

    assert response.status_code == 201
    data = response.json()
    assert data["status"] == "Success"
    assert "payment" in data
    assert data["payment"]["hash"].startswith("REFUND:")


def test_put_payment_no_auth(_data, setup_payment):
    url = f"{_data['url']}/{setup_payment}"
    response = requests.put(url, json={"t_data": {"info": "ok"}, "validation": "hash"})
    assert response.status_code == 401


def test_put_payment_missing_field(_data, user_headers, setup_payment):
    url = f"{_data['url']}/{setup_payment}"
    data = {"t_data": {"info": "ok"}}  # 'validation' mist
    response = requests.put(url, headers=user_headers, json=data)
    assert response.status_code == 400
    body = response.json()
    assert "error" in body
    assert "validation" in body["error"]


def test_put_payment_invalid_hash(_data, user_headers, setup_payment):
    url = f"{_data['url']}/{setup_payment}"
    data = {"t_data": {"info": "ok"}, "validation": "invalid_hash"}
    response = requests.put(url, headers=user_headers, json=data)

    assert response.status_code == 401
    assert "Validation failed" in response.json()["error"]


def test_put_payment_success(_data, user_headers, setup_payment):
    url = f"{_data['url']}/{setup_payment}"
    data = {"t_data": {"info": "complete"}, "validation": "hash123"}
    response = requests.put(url, headers=user_headers, json=data)

    assert response.status_code == 200
    data = response.json()
    assert data["status"] == "Success"
    assert "payment" in data
    assert data["payment"]["hash"] == "hash123"
