import pytest
import requests
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

# @pytest.fixture   
# def _data():
#     BASE = "http://localhost:8000"
#     r = requests.post(f"{BASE}/login", json={"username": "login.user", "password": "Password123."})
    
#     return {
#         "url": "http://localhost:8000/profile",
#         "user_token": r.json().get("accesstoken"),
#         "admin_token": "adminToken123"
#     }

def register_and_login(base_url, user):
    requests.post(f"{base_url}/register", json=user)

    r = requests.post(
        f"{base_url}/login",
        json={"username": user["username"], "password": user["password"]},
    )

    if r.status_code != 200 or "accesstoken" not in r.json():
        pytest.fail(
            f"Fout bij inloggen: {r.status_code} - {r.text} {r.json()}"
        )

    body = r.json()
    return body['accesstoken']

@pytest.fixture
def user_headers(_data):
    return {"Authorization": f'Bearer {register_and_login(_data["base"], _data["users"]["user_a"])}'}

@pytest.fixture
def admin_headers(_data):
    return {"Authorization": f'Bearer {register_and_login(_data["base"], _data["users"]["admin"])}'}

def test_profile_no_auth(_data):
    r = requests.get(_data["base"] + "/profile")
    assert r.status_code in [401, 403]

def test_profile_user_ok(_data, user_headers):
    r = requests.get(_data["base"] + "/profile", headers=user_headers)
    print(f"hellolb{r.text}{user_headers}")
    assert r.status_code == 200

def test_profile_invalid_token(_data):
    r = requests.get(_data["base"] + "/profile", headers={"Authorization": "invalid"})
    assert r.status_code in [400, 401, 403]

def test_profile_after_logout(_data, user_headers):
    requests.get(_data["base"] + "/logout", headers=user_headers)
    r = requests.get(_data["base"] + "/profile", headers=user_headers)
    assert r.status_code in [200, 401, 403]