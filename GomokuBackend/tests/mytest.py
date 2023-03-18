from player import Player


def test1():
    player1 = Player("1", "http://localhost:8000")
    player2 = Player("2", "http://localhost:8000")

    player1.getHello()
    player1.postGamestart(1)
    player2.postGamestart(2)


if __name__ == "__main__":
    test1()