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

def test_register_success(_data):
    r = requests.post(_data["base"]+ "/register", json={
        "username": _data["users"]["user_b"]["username" + "2"],
        "password": _data["users"]["user_b"]["password"+ "2"],
        "name": _data["users"]["user_b"]["name"+ "2"],
        "email": _data["users"]["user_b"]["2" + "email"]
    })
    assert r.status_code in [200, 201]

def test_register_missing_username(_data):
    r = requests.post(_data["base"]+ "/register", json={
        "password": _data["users"]["user_a"]["password"],
        "name": _data["users"]["user_a"]["name"]
    })
    assert r.status_code in [400, 422]

def test_register_missing_password(_data):
    r = requests.post(_data["base"]+ "/register", json={"username": _data["users"]["user_a"]["username"], "name": _data["users"]["user_a"]["name"]})
    assert r.status_code in [400, 422]

def test_register_missing_name(_data):
    r = requests.post(_data["base"]+ "/register", json={"username": _data["users"]["user_a"]["username"], "password": _data["users"]["user_a"]["password"]})
    assert r.status_code in [400, 422]

def test_register_invalid_json(_data):
    r = requests.post(_data["base"]+ "/register", data="{ invalid json")
    assert r.status_code in [400, 415]

def test_register_duplicate_username(_data):
    payload = {"username": _data["users"]["user_a"]["username"], "password": _data["users"]["user_a"]["password"], "name": _data["users"]["user_a"]["name"]}
    requests.post(_data["base"]+ "/register", json=payload)
    r2 = requests.post(_data["base"]+ "/register", json=payload)
    assert r2.status_code in [409, 400, 500]

def test_register_weak_password(_data):
    r = requests.post(_data["base"]+ "/register", json={
        "username": _data["users"]["user_a"]["username"],
        "password": "123",
        "name": "Weak Password"
    })
    print(f"hellol{r.text}")
    assert r.status_code in [400, 422]

def test_register_wrong_content_type(_data):
    r = requests.post(_data["base"]+ "/register", data="username=test&password=1234")
    assert r.status_code in [400, 415]

def test_register_short_password(_data):
    r = requests.post(_data["base"]+ "/register", json={
        "username": _data["users"]["user_a"]["username"],
        "password": "1234",
        "name": "Short Pass"
    })
    print(f"hell{r.text}")
    assert r.status_code in [400, 422]
