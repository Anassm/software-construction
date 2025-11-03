import pytest
import requests


@pytest.fixture
def _data():
    return {
        "url": "http://localhost:8000/payments",
        "user_token": "userToken123",
        "admin_token": "adminToken123"
    }


@pytest.fixture
def user_headers(_data):
    return {
        "Authorization": _data["user_token"],
        "Content-Type": "application/json"
    }


@pytest.fixture
def admin_headers(_data):
    return {
        "Authorization": _data["admin_token"],
        "Content-Type": "application/json"
    }


# GET
def test_get_payments_no_auth(_data):
    r = requests.get(_data["url"])
    assert r.status_code == 401
    assert "Unauthorized" in r.text


def test_get_payments_user(_data, user_headers):
    r = requests.get(_data["url"], headers=user_headers)
    assert r.status_code == 200
    assert isinstance(r.json(), list)


def test_get_payments_invalid_token(_data):
    r = requests.get(_data["url"], headers={"Authorization": "invalid"})
    assert r.status_code == 401


def test_get_payments_by_username_admin(_data, admin_headers):
    r = requests.get(f"{_data['url']}/testuser", headers=admin_headers)
    assert r.status_code in [200, 404]
    if r.status_code == 200:
        assert isinstance(r.json(), list)


def test_get_payments_by_username_user_forbidden(_data, user_headers):
    r = requests.get(f"{_data['url']}/someoneelse", headers=user_headers)
    assert r.status_code == 403


# POST
def test_post_payment_no_auth(_data):
    data = {"transaction": "tx1", "amount": 100}
    r = requests.post(_data["url"], json=data)
    assert r.status_code == 401


def test_post_payment_missing_field(_data, user_headers):
    data = {"amount": 50}
    r = requests.post(_data["url"], headers=user_headers, json=data)
    assert r.status_code == 400
    body = r.json()
    assert body["error"] == "Required field missing"
    assert body["field"] == "transaction"


def test_post_payment_invalid_json(_data, user_headers):
    r = requests.post(_data["url"], headers=user_headers, data="{ invalid json")
    assert r.status_code == 400


def test_post_payment_success(_data, user_headers):
    data = {"transaction": "tx1", "amount": 100}
    r = requests.post(_data["url"], headers=user_headers, json=data)
    assert r.status_code == 201
    data = r.json()
    assert data["status"] == "Success"
    assert "payment" in data


def test_post_refund_no_auth(_data):
    r = requests.post(f"{_data['url']}/refund", json={"transaction": "tx1", "amount": 10})
    assert r.status_code == 401


def test_post_refund_user_forbidden(_data, user_headers):
    r = requests.post(f"{_data['url']}/refund", headers=user_headers, json={"transaction": "tx1", "amount": 10})
    assert r.status_code == 403


def test_post_refund_admin_missing_field(_data, admin_headers):
    data = {"transaction": "tx2"}
    r = requests.post(f"{_data['url']}/refund", headers=admin_headers, json=data)
    assert r.status_code == 400
    body = r.json()
    assert body["error"] == "Required field missing"
    assert body["field"] == "amount"


def test_post_refund_admin_success(_data, admin_headers):
    data = {"transaction": "tx2", "amount": 10}
    r = requests.post(f"{_data['url']}/refund", headers=admin_headers, json=data)
    assert r.status_code == 201
    data = r.json()
    assert data["status"] == "Success"
    assert "payment" in data


# PUT
def test_put_payment_no_auth(_data):
    url = f"{_data['url']}/tx1"
    r = requests.put(url, json={"t_data": {"info": "ok"}, "validation": "hash"})
    assert r.status_code == 401


def test_put_payment_missing_field(_data, user_headers):
    url = f"{_data['url']}/tx1"
    data = {"t_data": {"info": "ok"}}
    r = requests.put(url, headers=user_headers, json=data)
    assert r.status_code == 400
    body = r.json()
    assert body["error"] == "Required field missing"
    assert body["field"] == "validation"


def test_put_payment_invalid_hash(_data, user_headers):
    url = f"{_data['url']}/tx1"
    data = {"t_data": {"info": "ok"}, "validation": "invalid_hash"}
    r = requests.put(url, headers=user_headers, json=data)
    assert r.status_code == 401
    assert "Validation failed" in r.text


def test_put_payment_success(_data, user_headers):
    url = f"{_data['url']}/tx1"
    data = {"t_data": {"info": "complete"}, "validation": "hash123"}
    r = requests.put(url, headers=user_headers, json=data)
    assert r.status_code == 200
    data = r.json()
    assert data["status"] == "Success"
    assert "payment" in data
