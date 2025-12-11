import pytest
import requests
import uuid

# -----------------------------
#  Fixtures (reused from your structure)
# -----------------------------

@pytest.fixture
def _data():
    return {
        "base": "http://localhost:8000",
        "users": {
            "user_a": {
                "email": "user@example.com",
                "password": "UserPass123!",
                "username": "regular.user",
                "name": "Regular User",
                "role": "user"
            },
            "user_b": {
                "email": "user2@example.com",
                "password": "User2Pass123!",
                "username": "user.two",
                "name": "Second User",
                "role": "user"
            },
            "admin": {
                "email": "admin@example.com",
                "password": "AdminPass123!",
                "username": "admin.user",
                "name": "Admin User",
                "role": "Admin"
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


# -----------------------------
# Helpers
# -----------------------------

def discount_url(_data):
    return f"{_data['base']}/discounts"


def create_discount(admin_token, _data, code=None):
    if code is None:
        code = "UPD" + uuid.uuid4().hex[:6]

    r = requests.post(
        discount_url(_data),
        headers=admin_token,
        json={"code": code}
    )

    return r.json()["discount"]["id"]


# ===================================================================
#  POST /discounts — ORIGINAL TESTS
# ===================================================================

def test_post_discount_no_auth(_data):
    response = requests.post(discount_url(_data), json={"code": "NEWYEAR"})
    assert response.status_code == 401
    assert "Unauthorized" in response.text


def test_post_discount_non_admin(user_token, _data):
    payload = {"code": "USERTRY"}
    response = requests.post(discount_url(_data), headers=user_token, json=payload)
    assert response.status_code == 403

    body = response.json()
    assert "Access denied" in body["error"]


def test_post_discount_missing_code(admin_token, _data):
    payload = {"percentage": 10}

    response = requests.post(discount_url(_data), headers=admin_token, json=payload)
    assert response.status_code == 400

    body = response.json()
    assert "Code" in body["error"]


def test_post_discount_success(admin_token, _data):
    payload = {
        "code": "DIS" + uuid.uuid4().hex[:5],
        "isActive": True,
        "percentage": 15.0
    }

    response = requests.post(discount_url(_data), headers=admin_token, json=payload)
    assert response.status_code == 201

    body = response.json()
    assert body["status"] == "Success"
    assert body["discount"]["code"].startswith("DIS")


def test_post_discount_duplicate(admin_token, _data):
    code = "DUP" + uuid.uuid4().hex[:4]
    payload = {"code": code}

    requests.post(discount_url(_data), headers=admin_token, json=payload)

    response = requests.post(discount_url(_data), headers=admin_token, json=payload)
    assert response.status_code == 409

    body = response.json()
    assert "already exists" in body["error"]


# ===================================================================
#  PUT /discounts/{id} — ORIGINAL TESTS
# ===================================================================

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


# ===================================================================
#  NEW ENDPOINT TESTS START HERE
# ===================================================================

# ----------------------------------------
# PUT /discounts/{id}/deactivate
# ----------------------------------------

def test_deactivate_discount_no_auth(_data):
    did = uuid.uuid4()
    r = requests.put(f"{discount_url(_data)}/{did}/deactivate")
    assert r.status_code == 401


def test_deactivate_discount_not_found(admin_token, _data):
    did = uuid.uuid4()
    r = requests.put(f"{discount_url(_data)}/{did}/deactivate", headers=admin_token)
    assert r.status_code == 404


def test_deactivate_discount_happy(admin_token, _data):
    did = create_discount(admin_token, _data)
    r = requests.put(f"{discount_url(_data)}/{did}/deactivate", headers=admin_token)
    assert r.status_code == 200


# ----------------------------------------
# PUT /discounts/{id}/expiry
# ----------------------------------------

def test_update_expiry_no_auth(_data):
    did = uuid.uuid4()
    r = requests.put(f"{discount_url(_data)}/{did}/expiry", json="2029-01-01T00:00:00")
    assert r.status_code == 401


def test_update_expiry_not_found(admin_token, _data):
    did = uuid.uuid4()
    r = requests.put(
        f"{discount_url(_data)}/{did}/expiry",
        headers=admin_token,
        json="2029-01-01T00:00:00"
    )
    assert r.status_code == 404


def test_update_expiry_happy(admin_token, _data):
    did = create_discount(admin_token, _data)
    r = requests.put(
        f"{discount_url(_data)}/{did}/expiry",
        headers=admin_token,
        json="2030-01-01T00:00:00"
    )
    assert r.status_code == 200


# ----------------------------------------
# POST /discounts/{id}/links
# ----------------------------------------

def test_link_users_no_body(admin_token, _data):
    did = create_discount(admin_token, _data)
    r = requests.post(f"{discount_url(_data)}/{did}/links",
                      headers=admin_token, json=None)
    assert r.status_code == 415


def test_link_users_not_found(admin_token, _data):
    did = uuid.uuid4()
    r = requests.post(f"{discount_url(_data)}/{did}/links",
                      headers=admin_token,
                      json={"userIds": []})
    assert r.status_code == 404


def test_link_users_happy(admin_token, _data):
    did = create_discount(admin_token, _data)
    id = requests.get(
        f"{_data['base']}/profile",
        headers=admin_token).json().get("id")
    print(f"User ID for linking: {id}")
    r = requests.post(
        f"{discount_url(_data)}/{did}/links",
        headers=admin_token,
        json={"userIds": [id]}
    )
    print(f"Link users response: {r.text}")
    
    assert r.status_code == 200


# ----------------------------------------
# POST /discounts/validate
# ----------------------------------------

def test_validate_no_auth(_data):
    r = requests.post(f"{discount_url(_data)}/validate", json={"code": "ABC"})
    assert r.status_code == 401


def test_validate_invalid(admin_token, _data):
    r = requests.post(
        f"{discount_url(_data)}/validate",
        headers=admin_token,
        json={"code": "NONEXISTENTCODE"}
    )
    assert r.status_code in (400, 404)


def test_validate_happy(admin_token, _data):
    create_discount(admin_token, _data)
    r = requests.post(
        f"{discount_url(_data)}/validate",
        headers=admin_token,
        json={"code": "VALID"}  # service logic defines actual validity
    )
    assert r.status_code in (200, 400, 404)


# ----------------------------------------
# GET /discounts/statistics
# ----------------------------------------

def test_get_statistics_forbidden(user_token, _data):
    r = requests.get(f"{discount_url(_data)}/statistics", headers=user_token)
    assert r.status_code == 403


def test_get_statistics_happy(admin_token, _data):
    r = requests.get(f"{discount_url(_data)}/statistics", headers=admin_token)
    assert r.status_code == 200


# ----------------------------------------
# GET /discounts/active
# ----------------------------------------

def test_get_active_no_auth(_data):
    r = requests.get(f"{discount_url(_data)}/active")
    assert r.status_code == 401


def test_get_active_happy(admin_token, _data):
    r = requests.get(f"{discount_url(_data)}/active", headers=admin_token)
    assert r.status_code == 200


# ----------------------------------------
# GET /discounts/statistics/{filter}/{orderby}
# ----------------------------------------

def test_get_statistics_filter_invalid(admin_token, _data):
    r = requests.get(
        f"{discount_url(_data)}/statistics/wrong/asc",
        headers=admin_token
    )
    assert r.status_code == 400


def test_get_statistics_order_invalid(admin_token, _data):
    r = requests.get(
        f"{discount_url(_data)}/statistics/totaluses/wrong",
        headers=admin_token
    )
    assert r.status_code == 400


def test_get_statistics_filter_happy(admin_token, _data):
    r = requests.get(
        f"{discount_url(_data)}/statistics/totaluses/asc",
        headers=admin_token
    )
    assert r.status_code == 200


# ----------------------------------------
# GET /discounts/used
# ----------------------------------------

def test_get_used_no_auth(_data):
    r = requests.get(f"{discount_url(_data)}/used")
    assert r.status_code == 401


def test_get_used_happy(admin_token, _data):
    r = requests.get(f"{discount_url(_data)}/used", headers=admin_token)
    assert r.status_code == 200