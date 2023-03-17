# from django.contrib.auth.models import AnonymousUser, User
from django.test import RequestFactory, TestCase
# from django.test.runner import DiscoverRunner

from app.views import hello, pregame, ingame, postgame


class Player():
    def __init__(self, name: str):
        self.factory = RequestFactory()
        self.name = name
        self.userid = None
        self.pieces = []

    def getHello(self):
        request = self.factory.get('/hello')
        response = hello.hello(request)
        print(f"Player {self.name} {request.method} {request.path} response {response.content.decode('utf-8')}")

    def postGamestart(self, ruleid):
        form = {'ruleid': ruleid}
        request = self.factory.post('/gamestart', form)
        print(f"\nPlayer {self.name} {request.method} {request.path} {form}")
        response = pregame.gamestart(request)
        print(f"Player {self.name} {request.method} {request.path} response {response.content.decode('utf-8')}")
        assert(response.status_code == 200)

        self.userid = response.get('userid')
        print(f"Player {self.name} userid set to {self.userid}")

    def postWaitformatch(self):
        form = {'userid': self.userid}
        request = self.factory.post('/waitformatch', form)
        print(f"\nPlayer {self.name} {request.method} {request.path} {form}")
        response = ingame.waitformatch(request)
        print(f"Player {self.name} {request.method} {request.path} response {response.content.decode('utf-8')}")
        assert(response.status_code == 200)

    def postSendpiece(self, piece):
        form = {'userid': self.userid, 'marker_location': piece}
        request = self.factory.post('/sendpiece', form)
        print(f"\nPlayer {self.name} {request.method} {request.path} {form}")
        response = ingame.sendpiece(request)
        print(f"Player {self.name} {request.method} {request.path} response {response.content.decode('utf-8')}")
        assert(response.status_code == 200)

        self.pieces.append(piece)

    def postCheckstatus(self):
        form = {'userid': self.userid}
        request = self.factory.post('/checkstatus', form)
        print(f"\nPlayer {self.name} {request.method} {request.path} {form}")
        response = ingame.checkstatus(response)
        print(f"Player {self.name} {request.method} {request.path} response {response.content.decode('utf-8')}")
        assert(response.status_code == 200)

    def postEndgame(self):
        form = {'userid': self.userid}
        request = self.factory.post('/endgame', form)
        print(f"\nPlayer {self.name} {request.method} {request.path} {form}")
        response = ingame.endgame(request)
        print(f"Player {self.name} {request.method} {request.path} response {response.content.decode('utf-8')}")
        assert(response.status_code == 200)

    def postCheckwin(self):
        form = {'userid': self.userid}
        request = self.factory.post('/checkwin', form)
        print(f"\nPlayer {self.name} {request.method} {request.path} {form}")
        response = postgame.checkwin(request)
        print(f"Player {self.name} {request.method} {request.path} response {response.content.decode('utf-8')}")
        assert(response.status_code == 200)

    def postClearrecords(self):
        form = {'userid': self.userid}
        request = self.factory.post('/clearrecords', form)
        print(f"\nPlayer {self.name} {request.method} {request.path} {form}")
        response = postgame.clearrecords(request)
        print(f"Player {self.name} {request.method} {request.path} response {response.content.decode('utf-8')}")
        assert(response.status_code == 200)


class SimpleTest(TestCase):
    def setUp(self):
        pass

    def test_details(self):
        player1 = Player('1')
        player2 = Player('2')

        player1.postGamestart(1)
        player2.postGamestart(1)
