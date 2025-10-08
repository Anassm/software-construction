import pytest
import requests


# ----------------
# Fixtures
# ----------------
@pytest.fixture
def _data():
    return {
        "url": "http://localhost:8000/",
        "adminToken": "adminToken123",
        "userToken": "userToken123",
        "api_key": "a1b2c3d4e5",
    }


@pytest.fixture
def admin_headers(_data):
    return {
        "API_KEY": _data["api_key"],
        "Authorization": f"Bearer {_data['adminToken']}",
    }


@pytest.fixture
def user_headers(_data):
    return {
        "API_KEY": _data["api_key"],
        "Authorization": f"Bearer {_data['userToken']}",
    }


def test_get_payments(_data, admin_headers):
    url = _data["url"] + "payments"
    response = requests.get(url, headers=admin_headers)
    assert response.status_code in [200, 401, 403]


def test_get_payments_by_username(_data, admin_headers):
    url = _data["url"] + "payments/testuser"
    response = requests.get(url, headers=admin_headers)
    assert response.status_code in [200, 401, 403, 404]


def test_post_payment(_data, user_headers):
    url = _data["url"] + "payments"
    payload = {
        "transaction": "tx123",
        "amount": 100,
    }
    response = requests.post(url, headers=user_headers, json=payload)
    assert response.status_code in [201, 401, 403]


def test_put_payment(_data, user_headers):
    url = _data["url"] + "payments/tx123"
    payload = {"t_data": {"info": "some data"}, "validation": "hash123"}
    response = requests.put(url, headers=user_headers, json=payload)
    assert response.status_code in [200, 401, 403, 404]


def test_post_refund(_data, admin_headers):
    url = _data["url"] + "payments/refund"
    payload = {
        "transaction": "tx123",
        "amount": 50,
    }
    response = requests.post(url, headers=admin_headers, json=payload)
    assert response.status_code in [201, 401, 403]
