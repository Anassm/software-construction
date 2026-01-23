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

    r = requests.post(f"{base_url}/login", json={
        "username": user["username"],
        "password": user["password"]
    })
    body = r.json()

    return {"Authorization": f"{body['tokentype']} {body['accesstoken']}"}


@pytest.fixture
def user_token(_data):
    return register_and_login(_data["base"], _data["users"]["user_a"])


@pytest.fixture
def user_token_b(_data):
    return register_and_login(_data["base"], _data["users"]["user_b"])


@pytest.fixture
def admin_token(_data):
    return register_and_login(_data["base"], _data["users"]["admin"])

def discount_url(_data):
    return f"{_data['base']}/discounts"


def create_discount(admin_token, _data, code=None):
    if code is None:
        code = "UPD" + uuid.uuid4().hex[:6].upper()

    r = requests.post(
        discount_url(_data),
        headers=admin_token,
        json={
            "code": code,
            "percentage": 10,  # ✅ ADD required field
            "isActive": True
        }
    )

    return r.json()["discount"]["id"]

def test_post_discount_no_auth(_data):
    response = requests.post(discount_url(_data), json={"code": "NEWYEAR"})
    assert response.status_code == 401
    assert "Unauthorized" in response.text


def test_post_discount_non_admin(user_token, _data):
    payload = {"code": "USERTRY", "percentage": 5}
    response = requests.post(discount_url(_data), headers=user_token, json=payload)
    assert response.status_code == 403
    
    body = response.json()
    assert "Access denied" in body["error"]


def test_post_discount_missing_code(admin_token, _data):
    payload = {"percentage": 10}

    response = requests.post(discount_url(
        _data), headers=admin_token, json=payload)
    assert response.status_code == 400

    body = response.json()
    assert "code" in body["error"]


def test_post_discount_success(admin_token, _data):
    payload = {
        "code": "DIS" + uuid.uuid4().hex[:5].upper(),  
        "isActive": True,
        "percentage": 15.0
    }

    response = requests.post(discount_url(
        _data), headers=admin_token, json=payload)
    assert response.status_code == 201

    body = response.json()
    assert body["status"] == "Success"
    assert body["discount"]["code"].startswith("DIS")


def test_post_discount_duplicate(admin_token, _data):
    code = "DUP" + uuid.uuid4().hex[:4].upper()
    payload = {"code": code, "percentage": 5}

    requests.post(discount_url(_data), headers=admin_token, json=payload)

    response = requests.post(discount_url(
        _data), headers=admin_token, json=payload)
    assert response.status_code == 409

    body = response.json()
    assert "already exists" in body["error"]

def test_put_discount_no_auth(_data):
    fake_id = uuid.uuid4()
    response = requests.put(
        f"{discount_url(_data)}/{fake_id}",
        json={"isActive": False}
    )
    assert response.status_code == 401


def test_put_discount_non_admin(user_token, _data):
    fake_id = uuid.uuid4()
    response = requests.put(
        f"{discount_url(_data)}/{fake_id}",
        headers=user_token,
        json={"isActive": False}
    )
    assert response.status_code == 403
    assert "Admin role" in response.text


def test_put_discount_not_found(admin_token, _data):
    fake_id = uuid.uuid4()
    response = requests.put(
        f"{discount_url(_data)}/{fake_id}",
        headers=admin_token,
        json={"isActive": False}
    )
    assert response.status_code == 404
    assert "not found" in response.text


def test_put_discount_success(admin_token, _data):
    did = create_discount(admin_token, _data)

    response = requests.put(
        f"{discount_url(_data)}/{did}",
        headers=admin_token,
        json={"isActive": False, "percentage": 20}
    )

    assert response.status_code == 200
    body = response.json()

    assert body["status"] == "Success"
    assert body["discount"]["isActive"] is False
    assert body["discount"]["percentage"] == 20


def test_put_discount_partial_update(admin_token, _data):
    did = create_discount(admin_token, _data)

    response = requests.put(
        f"{discount_url(_data)}/{did}",
        headers=admin_token,
        json={"allowedLocation": "Rotterdam"}
    )

    assert response.status_code == 200
    body = response.json()

    assert body["discount"]["allowedLocation"] == "Rotterdam"
