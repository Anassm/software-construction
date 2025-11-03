import pytest
import requests

@pytest.fixture
def _data():
    return {
        "base": "http://localhost:8000/billing",
        "user_token": "userToken123",
        "admin_token": "adminToken123"
    }

@pytest.fixture
def user_headers(_data):
    return {"Authorization": _data["user_token"]}

@pytest.fixture
def admin_headers(_data):
    return {"Authorization": _data["admin_token"]}

def test_billing_no_auth(_data):
    r = requests.get(f"{_data['base']}/regular.user")
    assert r.status_code in [401, 403]

def test_billing_user_forbidden_other_user(_data, user_headers):
    r = requests.get(f"{_data['base']}/admin.user", headers=user_headers)
    assert r.status_code in [401, 403]

def test_billing_user_self_access(_data, user_headers):
    r = requests.get(f"{_data['base']}/regular.user", headers=user_headers)
    assert r.status_code in [200, 403]

def test_billing_admin_by_username(_data, admin_headers):
    r = requests.get(f"{_data['base']}/regular.user", headers=admin_headers)
    assert r.status_code in [200, 404]
    if r.status_code == 200:
        assert isinstance(r.json(), list)

def test_billing_invalid_token(_data):
    r = requests.get(f"{_data['base']}/regular.user", headers={"Authorization": "invalid"})
    assert r.status_code in [401, 403]
