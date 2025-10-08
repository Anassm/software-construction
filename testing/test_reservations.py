import pytest
import requests
import uuid

@pytest.fixture
def _data():
    return {
        'url': 'http://localhost:8000/reservations/',
        'api_key': 'a1b2c3d4e5',
        'tokenAdmin': str(uuid.uuid4()),
        'tokenUser': str(uuid.uuid4()),
        'admin:': {"id":"2","username":"gijsdegraaf","password":"1b1f4e666f54b55ccd2c701ec3435dba","name":"Gijs de Graaf","email":"gijsdegraaf@hotmail.com","phone":"+310698086312","role":"ADMIN","created_at":"2017-07-10","birth_year":1951,"active":True},
        'user': {"id":"123","username":"janedoe","password":"e99a18c428cb38d5f260853678922e03","name":"Jane Doe","email":"","phone":"","role":"USER","created_at":"2020-01-15","birth_year":1990,"active":True}


    }



def test_get_reservations_admin(_data):
    tokenAdmin = _data['admin']['tokenAdmin']
    id = _data['user']['id']
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



    