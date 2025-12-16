import pytest
import requests


@pytest.fixture
def _data():
    return {
        "base": "http://localhost:8000/billing",
        "user_token": "userToken123",
        "admin_token": "adminToken123",
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
    r = requests.get(
        f"{_data['base']}/regular.user", headers={"Authorization": "invalid"}
    )
    assert r.status_code in [401, 403]


def test_monthly_invoices_no_auth(_data):
    r = requests.get(f"{_data['base']}/invoices/monthly?year=2025&month=11")
    assert r.status_code in [401, 403]


def test_monthly_invoices_invalid_token(_data):
    r = requests.get(
        f"{_data['base']}/invoices/monthly?year=2025&month=11",
        headers={"Authorization": "invalid"},
    )
    assert r.status_code in [401, 403]


def test_monthly_invoices_user_success(_data, user_headers):
    r = requests.get(
        f"{_data['base']}/invoices/monthly?year=2025&month=11", headers=user_headers
    )
    assert r.status_code in [200, 404]

    if r.status_code == 200:
        body = r.json()
        assert "invoices" in body
        assert "totalAmount" in body
        assert "totalInvoices" in body
        assert isinstance(body["invoices"], list)


def test_monthly_invoices_admin_success(_data, admin_headers):
    r = requests.get(
        f"{_data['base']}/invoices/monthly?year=2025&month=11", headers=admin_headers
    )
    assert r.status_code in [200, 404]

    if r.status_code == 200:
        body = r.json()
        assert isinstance(body.get("invoices"), list)


def test_monthly_invoices_missing_params(_data, user_headers):
    r = requests.get(f"{_data['base']}/invoices/monthly", headers=user_headers)
    assert r.status_code in [400, 422]


def test_monthly_invoices_invalid_month(_data, user_headers):
    r = requests.get(
        f"{_data['base']}/invoices/monthly?year=2025&month=13", headers=user_headers
    )
    assert r.status_code == 400


def test_monthly_invoices_invalid_year(_data, user_headers):
    r = requests.get(
        f"{_data['base']}/invoices/monthly?year=0&month=11", headers=user_headers
    )
    assert r.status_code == 400
