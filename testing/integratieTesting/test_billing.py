import os
import pytest
import requests

@pytest.fixture
def _data():
    return {
        "base": os.environ["BASE_URL"],
        "users": {
            "user": {
                "email": os.environ["BILLING_USER_EMAIL"],
                "password": os.environ["BILLING_USER_PASSWORD"],
                "username": os.environ["BILLING_USER_USERNAME"],
                "name": os.environ["BILLING_USER_NAME"],
                "role": "user"
            },
            "admin": {
                "email": os.environ["BILLING_ADMIN_EMAIL"],
                "password": os.environ["BILLING_ADMIN_PASSWORD"],
                "username": os.environ["BILLING_ADMIN_USERNAME"],
                "name": os.environ["BILLING_ADMIN_NAME"],
                "role": "admin"
            }
        }
    }

def register_and_login(base_url, user):
    requests.post(f"{base_url}/register", json=user)
    r = requests.post(
        f"{base_url}/login",
        json={"username": user["username"], "password": user["password"]}
    )
    body = r.json()
    return {
        "Authorization": f"{body['tokentype']} {body['accesstoken']}"
    }

@pytest.fixture
def user_token(_data):
    return register_and_login(_data["base"], _data["users"]["user"])

@pytest.fixture
def admin_token(_data):
    return register_and_login(_data["base"], _data["users"]["admin"])

def test_get_my_invoices_no_auth(_data):
    r = requests.get(f"{_data['base']}/billing/invoices")
    assert r.status_code == 401

def test_get_my_invoices_invalid_token(_data):
    r = requests.get(
        f"{_data['base']}/billing/invoices",
        headers={"Authorization": "invalid-token"}
    )
    assert r.status_code == 401

def test_get_my_invoices_success(_data, user_token):
    r = requests.get(
        f"{_data['base']}/billing/invoices",
        headers=user_token
    )
    assert r.status_code == 200
    body = r.json()
    assert "invoices" in body
    assert isinstance(body["invoices"], list)

def test_monthly_invoices_no_auth(_data):
    r = requests.get(
        f"{_data['base']}/billing/invoices/monthly?year=2025&month=11"
    )
    assert r.status_code == 401

def test_monthly_invoices_invalid_token(_data):
    r = requests.get(
        f"{_data['base']}/billing/invoices/monthly?year=2025&month=11",
        headers={"Authorization": "invalid-token"}
    )
    assert r.status_code == 401

def test_monthly_invoices_missing_params(_data, user_token):
    r = requests.get(
        f"{_data['base']}/billing/invoices/monthly",
        headers=user_token
    )
    assert r.status_code == 400

def test_monthly_invoices_invalid_month(_data, user_token):
    r = requests.get(
        f"{_data['base']}/billing/invoices/monthly?year=2025&month=13",
        headers=user_token
    )
    assert r.status_code == 400

def test_monthly_invoices_invalid_year(_data, user_token):
    r = requests.get(
        f"{_data['base']}/billing/invoices/monthly?year=0&month=11",
        headers=user_token
    )
    assert r.status_code == 400

def test_monthly_invoices_user_success(_data, user_token):
    r = requests.get(
        f"{_data['base']}/billing/invoices/monthly?year=2025&month=11",
        headers=user_token
    )
    assert r.status_code == 200

    if r.status_code == 200:
        body = r.json()
        assert "invoices" in body
        assert "totalAmount" in body
        assert "totalInvoices" in body
        assert isinstance(body["invoices"], list)

def test_user_billing_summary_no_auth(_data):
    r = requests.get(
        f"{_data['base']}/billing/users/{os.environ['BILLING_USER_USERNAME']}/summary"
    )
    assert r.status_code == 401

def test_user_billing_summary_user_forbidden(_data, user_token):
    r = requests.get(
        f"{_data['base']}/billing/users/{os.environ['BILLING_USER_USERNAME']}/summary",
        headers=user_token
    )
    assert r.status_code == 403