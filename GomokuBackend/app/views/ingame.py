from django.shortcuts import render
from django.http import JsonResponse, HttpResponse
from django.db import connection
from django.views.decorators.csrf import csrf_exempt
import json


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
