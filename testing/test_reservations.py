import pytest
import requests


@pytest.fixture
def _data():
    return {
        'url': 'http://localhost:8000/reservations/',
        'api_key': 'a1b2c3d4e5',

    }


def test_get_reservations_admin(_data, admin_token):
    tokenAdmin = admin_token
    id = '1'
    response = requests.get(f"{_data['url']}{id}", headers={'Authorization' : tokenAdmin})

    status_code = response.status_code

    assert status_code == 200

def test_get_reservations_user(_data):
    tokenUser = 'userToken123'
    id = '123'
    response = requests.get(f"{_data['url']}{id}", headers={'Authorization' : tokenUser})

    status_code = response.status_code

    assert status_code == 200

def test_get_reservations_unauthorized(_data):
    id = '1'
    response = requests.get(f"{_data['url']}{id}")

    status_code = response.status_code

    assert status_code == 401

def test_get_reservations_access_denied(_data):
    tokenUser = 'userToken123'
    id = '124'
    response = requests.get(f"{_data['url']}{id}", headers={'Authorization' : tokenUser})

    status_code = response.status_code

    assert status_code == 403



    