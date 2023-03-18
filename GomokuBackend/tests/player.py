import requests


class Player:
    def __init__(self, name: str, host: str):
        self.name = name
        self.userid = None
        self.pieces = []
        self.url = host

    def get(self, path):
        print(f"\nPlayer {self.name} GET {path}")
        response = requests.get(self.url + path)
        data = response.json()
        print(f"Player {self.name} GET {path} response {data}")
        assert(response.status_code == 200)
        return data

    def post(self, path, form):
        print(f"\nPlayer {self.name} POST {path} {form}")
        response = requests.post(self.url + path, form)
        data = response.json()
        print(f"Player {self.name} POST {path} response {data}")
        assert(response.status_code == 200)
        return data

    def getHello(self):
        _ = self.get('/hello/')

    def postGamestart(self, ruleid):
        data = self.post('/gamestart/', {'ruleid': ruleid})
        self.userid = data['userid']
        print(f"Player {self.name} userid set to {self.userid}")

    def postWaitformatch(self):
        data = self.post('/waitformatch/', {'userid': self.userid})
        return data['isFirst']

    def postSendpiece(self, piece):
        _ = self.post('/sendpiece/', {'userid': self.userid, 'marker_location': str(piece)})
        self.pieces.append(piece)

    def postCheckstatus(self):
        _ = self.post('/checkstatus/', {'userid': self.userid})

    def postEndgame(self):
        _ = self.post('/endgame/', {'userid': self.userid})

    def postCheckwin(self):
        _ = self.post('/checkwin/', {'userid': self.userid})

    def postClearrecords(self):
        _ = self.post('/clearrecords/', {'userid': self.userid})
