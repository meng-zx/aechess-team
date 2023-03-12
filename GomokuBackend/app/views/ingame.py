from django.shortcuts import render
from django.http import JsonResponse, HttpResponse
from django.db import connection, transaction
from django.views.decorators.csrf import csrf_exempt
import json


@csrf_exempt
# @transaction.atomic
def gamestart(request):
    if request.method != 'POST':
        return HttpResponse(status=404)

    json_data = json.loads(request.body)
    response = {}



    ruleid = json_data['ruleid']

    cursor = connection.cursor()



    cursor.execute('SELECT userid FROM match_queue '
                   'WHERE ruleid = %s AND matched = FALSE;', (ruleid, ))

    opponentid = cursor.fetchone()

    if opponentid is None:
        cursor.execute('INSERT INTO match_queue(ruleid, matched) VALUES (%s, FALSE);'
                       , (ruleid, ))
        cursor.execute('SELECT MAX(userid) FROM match_queue;')
        userid = cursor.fetchone()
    else:
        opponentid = opponentid[0]
        cursor.execute('INSERT INTO match_queue(ruleid, matched) VALUES (%s, TRUE);'
                       , (ruleid, ))
        cursor.execute('UPDATE match_queue SET matched = TRUE '
                       'WHERE userid = %s', (opponentid, ))

        cursor.execute('SELECT MAX(userid) FROM match_queue;')
        userid = cursor.fetchone()
        cursor.execute('INSERT INTO game_info(userid, opponentid, player_turn, game_status, piece_cnt) VALUES'
                       '(%s, %s, %s, %s, %s);', (userid, opponentid, 'TRUE', 'OnGoing', 10))

    response['userid'] = userid
    return JsonResponse(response)


@csrf_exempt
def checkstatus(request):
    """Implements path('checkstatus/', ingame.checkstatus, name='checkstatus').
    """
    if request.method != 'POST':
        return HttpResponse(status=404)

    json_data = json.loads(request.body)
    userid = json_data['userid']
    gameid = json_data['gameid']

    cursor = connection.cursor()
    cursor.execute('SELECT game_status, player_turn FROM game_info '
                   'WHERE userid = %s AND gameid = %s;', (userid, gameid))

    status, turn = cursor.fetchone()
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
            cursor.execute('SELECT x, y, z FROM new_piece_info '
                'WHERE userid = %s AND gameid = %s;', (userid, gameid))
            new_piece = cursor.fetchone()
            # TODO: what if not found?
            new_piece = [float(s) for s in new_piece]
            response = {
                'status': "player turn",
                'new_piece_location': new_piece,
            }

    # TODO: if not found, return empty response?
    return JsonResponse(response)


@csrf_exempt
def endgame(request):
    """Implements path('endgame/', ingame.endgame, name='endgame').
    """
    if request.method != 'POST':
        return HttpResponse(status=404)

    json_data = json.loads(request.body)
    userid = json_data['userid']

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
    
    json_data = json.loads(request.body)
    userid = json_data['userid']
    gameid = json_data['gameid']
    marker_location = json_data['marker_location']
    response = {}
    cursor = connection.cursor()
    ## TODO: add error check for invalid gameid/userid?
    cursor.execute('SELECT game_status, opponentid FROM game_info '
                    'WHERE userid = %s AND gameid = %s;', (userid, gameid))
    status, opponentid = cursor.fetchone()
    if status in ["Win", "Lose"]:
        response = {
            'status': "end game"
        }
        return JsonResponse(response)
    if len(marker_location) == 3: 
        # check if already exist
        cursor.execute('SELECT x,y,z FROM NEW_PIECE_INFO '
                        'WHERE userid = %s AND gameid = %s;', (userid, gameid))
        exist = cursor.fetchall()
        if (len(exist)==0):
            cursor.execute('INSERT INTO NEW_PIECE_INFO (userid, gameid, x, y, z) VALUES (%s, %s, %s, %s, %s)'
                        , (userid, gameid, marker_location[0], marker_location[1], marker_location[2]))
        else:
            cursor.execute('UPDATE NEW_PIECE_INFO SET x = %s, y = %s, z = %s'
                    'WHERE userid = %s AND gameid = %s', (marker_location[0], marker_location[1], marker_location[2],userid, gameid))
        ## TODO: add check for duplicate location?
        cursor.execute('INSERT INTO ALL_PIECE_INFO (userid, gameid, x, y, z) VALUES'
                    '(%s, %s, %s, %s, %s)', (userid, gameid, marker_location[0], marker_location[1], marker_location[2]))
        cursor.execute('UPDATE GAME_INFO SET PIECE_CNT = PIECE_CNT + 1'
                    'WHERE userid = %s AND gameid = %s', (userid, gameid))
        ## TODO: MVP: implement checking end of game and update game status
        cursor.execute('UPDATE GAME_INFO SET PLAYER_TURN = %s '
                    'WHERE userid = %s AND gameid = %s', (False, userid, gameid))
        cursor.execute('UPDATE GAME_INFO SET PLAYER_TURN = %s '
                    'WHERE userid = %s AND gameid = %s', (True, opponentid, gameid))
    
    response = {
        'status': "opponent turn" 
    }
    return JsonResponse(response)

@csrf_exempt
def clearrecords(request):
    response = {}
    return JsonResponse(response)
