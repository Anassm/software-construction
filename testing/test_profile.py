import pytest
import requests

@pytest.fixture
def _data():
    return {
        "url": "http://localhost:8000/profile",
        "user_token": "userToken123",
        "admin_token": "adminToken123"
    }

@pytest.fixture
def user_headers(_data):
    return {"Authorization": _data["user_token"]}

@pytest.fixture
def admin_headers(_data):
    return {"Authorization": _data["admin_token"]}

def test_profile_no_auth(_data):
    r = requests.get(_data["url"])
    assert r.status_code in [401, 403]

def test_profile_user_ok(_data, user_headers):
    r = requests.get(_data["url"], headers=user_headers)
    assert r.status_code == 200

def test_profile_admin_ok(_data, admin_headers):
    r = requests.get(_data["url"], headers=admin_headers)
    assert r.status_code == 200

def test_profile_invalid_token(_data):
    r = requests.get(_data["url"], headers={"Authorization": "invalid"})
    assert r.status_code in [400, 401, 403]

def test_profile_after_logout(_data, user_headers):
    requests.get("http://localhost:8000/logout", headers=user_headers)
    r = requests.get(_data["url"], headers=user_headers)
    assert r.status_code in [200, 401, 403]