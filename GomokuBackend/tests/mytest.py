import time
from player import Player

player1 = Player("1", "http://localhost:8000")
player2 = Player("2", "http://localhost:8000")

def test1():
    player1.getHello()
    player1.postGamestart(0)
    player2.postGamestart(0)

    time.sleep(1)
    
    player1_turn = player1.postWaitformatch()
    player2_turn = player2.postWaitformatch()

    if player1_turn:
        print("player1 first")
        player1.postSendpiece((0.025, 0.0, 0.0))
    else:
        print("player2 first")

    # player2.postCheckstatus()
    player2.postSendpiece((0.0, 0.0, 0.025))
    player1.postCheckstatus()
    player1.postSendpiece((0.025, 0.0, 0.025))
    player1.postCheckstatus()
    
    player2.postCheckstatus()
    player2.postSendpiece((0.0, 0.0, 0.05))
    player1.postCheckstatus()
    player1.postSendpiece((0.05, 0.0, 0.05))

    player2.postCheckstatus()
    player2.postSendpiece((0.0, 0.0, 0.075))
    player1.postCheckstatus()
    player1.postSendpiece((0.025, 0.0, 0.075))

    player2.postCheckstatus()
    player2.postSendpiece((0.0, 0.0, 0.1))
    player1.postCheckstatus()
    player1.postSendpiece((-0.05, 0.0, 0.075))

    player2.postCheckstatus()
    player2.postSendpiece((0.0, 0.0, 0.125))

    player1.postCheckstatus()

    # player1.postCheckwin()
    # player1.postEndgame()

    # player2.postCheckstatus()
    # player1.postClearrecords()
    # player2.postClearrecords()

if __name__ == "__main__":
    test1()