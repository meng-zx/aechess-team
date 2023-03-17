from django.shortcuts import render
from django.http import JsonResponse, HttpResponse
from django.db import connection, transaction
from django.views.decorators.csrf import csrf_exempt
from urllib.parse import unquote
import json


@csrf_exempt
@transaction.atomic
def waitformatch(request):
    if request.method != 'POST':
        return HttpResponse(status=404)

    userid = request.POST.get('userid')
    response = {}

    cursor = connection.cursor()

    cursor.execute('SELECT userid, player_turn FROM game_info '
                   'WHERE userid = %s;', (userid, ))
    result = cursor.fetchone()

    if result is None:
        # response['gamid'] = -1
        response['isFirst'] = True
        response['status'] = "continue waiting"
    else:
        # response['gameid'] = result[1]
        response['isFirst'] = result[1]
        response['status'] = "matched"

    return JsonResponse(response)


@csrf_exempt
@transaction.atomic
def checkstatus(request):
    """Implements path('checkstatus/', ingame.checkstatus, name='checkstatus').
    """
    if request.method != 'POST':
        return HttpResponse(status=404)

    userid = request.POST.get('userid')
    response = {}

    cursor = connection.cursor()
    cursor.execute('SELECT opponentid, game_status, player_turn, piece_cnt FROM game_info '
                   'WHERE userid = %s;', (userid, ))

    opponentid, status, turn, count= cursor.fetchone()
    response = {}
    if status in ["Win", "Lose"]:
        response = {
            'status': status,
            'new_piece_location': [0, 0, 0],
        }
    elif status in ["OnGoing"]:
        if turn == False:
            response = {
                'status': "opponent turn",
                'new_piece_location': [0, 0, 0],
            }
        else:
            # find opponent's new piece if exists
            if count > 0:
                cursor.execute('SELECT x, y, z FROM new_piece_info '
                    'WHERE userid = %s;', (opponentid, ))
                new_piece = cursor.fetchone()
                # TODO: what if not found?
                new_piece = [float(s) for s in new_piece]
                response = {
                    'status': "player turn",
                    'new_piece_location': new_piece,
                }
            else:
                # TODO: return 0,0,0?
                response = {
                    'status': "player turn",
                    'new_piece_location': [0, 0, 0],
                }


    # TODO: if not found, return empty response?
    return JsonResponse(response)


@csrf_exempt
def endgame(request):
    """Implements path('endgame/', ingame.endgame, name='endgame').
    """
    if request.method != 'POST':
        return HttpResponse(status=404)

    userid = request.POST.get('userid')

    cursor = connection.cursor()
    cursor.execute('SELECT opponentid FROM game_info '
                   'WHERE userid = %s;', (userid, ))
    opponent = cursor.fetchall()
    # TODO: assume all ended matches are removed from db?
    if len(opponent) > 0:
        cursor.execute('UPDATE game_info SET game_status = %s '
                       'WHERE userid = %s', ('Win', opponent[0]))
        cursor.execute('UPDATE game_info SET game_status = %s '
                       'WHERE userid = %s', ('Lose', userid))
    else:
        # TODO: assume ongoing matches are removed from the queue?
        cursor.execute('UPDATE match_queue SET matched = True '
                       'WHERE userid = %s', (userid, ))

    response = {
        'status': "ended",
    }

    return JsonResponse(response)


@csrf_exempt
def sendpiece(request):
    """Implements path('sendpiece/', ingame.sendpiece, name='sendpiece').
    """
    if request.method != 'POST':
        return HttpResponse(status=404)

    userid = request.POST.get('userid')
    # TODO: modify marker_location
    marker_location = request.POST.get('marker_location') # example: '(0.04%2c%20-0.08%2c%200.05)'
    # with open('/home/ubuntu/aechess-team/GomokuBackend/log/2.txt', 'w') as file:
    #     file.write(request.body.decode('utf-8'))
    print(request.body.decode('utf-8'))
    print(marker_location)
    marker_location = unquote(marker_location) # example: '(0.04, -0.08, 0.05)'
    print(marker_location)
    print(len(marker_location))
    response = {}

    cursor = connection.cursor()
    ## TODO: add error check for invalid gameid/userid?
    cursor.execute('SELECT game_status, opponentid FROM game_info '
                    'WHERE userid = %s;', (userid, ))
    status, opponentid = cursor.fetchone()
    print(status)
    if status in ["Win", "Lose"]:
        response = {
            'status': "end game"
        }
        return JsonResponse(response)
    if len(marker_location) == 3:
        # check if already exist
        cursor.execute('SELECT x,y,z FROM NEW_PIECE_INFO '
                        'WHERE userid = %s;', (userid, ))
        exist = cursor.fetchall()
        if (len(exist)==0):
            cursor.execute('INSERT INTO NEW_PIECE_INFO (userid, x, y, z) VALUES (%s, %s, %s, %s);'
                        , (userid, marker_location[0], marker_location[1], marker_location[2]))
        else:
            cursor.execute('UPDATE NEW_PIECE_INFO SET x = %s, y = %s, z = %s'
                    'WHERE userid = %s;', (marker_location[0], marker_location[1], marker_location[2],userid))
        ## TODO: add check for duplicate location?
        cursor.execute('INSERT INTO ALL_PIECE_INFO (userid, x, y, z) VALUES'
                    '(%s, %s, %s, %s)', (userid, marker_location[0], marker_location[1], marker_location[2]))
        cursor.execute('UPDATE GAME_INFO SET PIECE_CNT = PIECE_CNT + 1'
                    'WHERE userid = %s;', (userid, ))
        ## TODO: MVP: implement checking end of game and update game status
        cursor.execute('UPDATE GAME_INFO SET PLAYER_TURN = %s '
                    'WHERE userid = %s;', (False, userid))
        cursor.execute('UPDATE GAME_INFO SET PLAYER_TURN = %s '
                    'WHERE userid = %s;', (True, opponentid))

    response = {
        'status': "opponent turn"
    }
    return JsonResponse(response)
