import pytest
import requests
import uuid


@pytest.fixture
def _data():
    return {
        "url": "http://localhost:8000/parkinglots",
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


def parkinglot_payload(name=None):
    return {
        "name": name or f"Test Lot {uuid.uuid4()}",
        "address": "Some Street 1",
        "location": "Rotterdam",
        "capacity": 10,
        "tariff": 2.5,
        "dayTariff": 10.0,
        "latitude": 51.9,
        "longitude": 4.48
    }


def test_get_parkinglots_no_auth(_data):
    r = requests.get(_data["url"])
    assert r.status_code == 200
    body = r.json()
    assert isinstance(body, list) or isinstance(body, dict)


def test_get_parkinglots_user(_data, user_headers):
    r = requests.get(_data["url"], headers=user_headers)
    assert r.status_code == 200


def test_post_parkinglot_admin_success(_data, admin_headers):
    r = requests.post(_data["url"], headers=admin_headers, json=parkinglot_payload())
    assert r.status_code in (200, 201)
    body = r.json()
    pl_id = body.get("id") or body.get("parkingLot", {}).get("id")
    assert pl_id is not None


def test_post_parkinglot_missing_field(_data, admin_headers):
    bad_payload = {
        "address": "No name street",
        "location": "Rotterdam",
        "capacity": 5,
        "tariff": 2.0,
        "dayTariff": 8.0,
        "latitude": 51.9,
        "longitude": 4.48
    }
    r = requests.post(_data["url"], headers=admin_headers, json=bad_payload)
    assert r.status_code in (400, 422)


def test_get_parkinglot_by_id_not_found(_data, admin_headers):
    r = requests.get(f"{_data['url']}/00000000-0000-0000-0000-000000000000", headers=admin_headers)
    assert r.status_code in (404, 400)


def test_put_parkinglot_admin_success(_data, admin_headers):
    create = requests.post(_data["url"], headers=admin_headers, json=parkinglot_payload())
    assert create.status_code in (200, 201)
    data = create.json()
    pid = data.get("id") or data.get("parkingLot", {}).get("id")

   
    update_payload = parkinglot_payload("Updated Name")
    r = requests.put(f"{_data['url']}/{pid}", headers=admin_headers, json=update_payload)

    assert r.status_code in (200, 204, 409), r.text


def test_put_parkinglot_not_found(_data, admin_headers):
    r = requests.put(
        f"{_data['url']}/11111111-1111-1111-1111-111111111111",
        headers=admin_headers,
        json=parkinglot_payload("DoesNotExist")
    )
    assert r.status_code in (404, 400)


def test_delete_parkinglot_admin_success(_data, admin_headers):
    create = requests.post(_data["url"], headers=admin_headers, json=parkinglot_payload())
    assert create.status_code in (200, 201)
    data = create.json()
    pid = data.get("id") or data.get("parkingLot", {}).get("id")

    r = requests.delete(f"{_data['url']}/{pid}", headers=admin_headers)
    assert r.status_code in (200, 204), r.text


def test_delete_parkinglot_not_found(_data, admin_headers):
    r = requests.delete(f"{_data['url']}/99999999-9999-9999-9999-999999999999", headers=admin_headers)
    assert r.status_code in (404, 400)
