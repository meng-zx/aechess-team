from django.shortcuts import render
from django.http import JsonResponse, HttpResponse
from django.db import connection, transaction
from django.views.decorators.csrf import csrf_exempt
import json


@csrf_exempt
@transaction.atomic
def clearrecords(request):
    if request.method != 'POST':
        return HttpResponse(status=404)

    userid = request.POST.get('userid')
    response = {}

    cursor = connection.cursor()
    cursor.execute('DELETE FROM match_queue '
                   'WHERE userid = %s;', (userid, ))

    cursor.execute('SELECT userid, opponentid FROM game_info '
                   'WHERE userid = %s OR opponentid = %s;', (userid, userid))
    result = cursor.fetchone()

    if result is not None:
        opponentid = result[0] + result[1] - userid
        if opponentid == -1:
            cursor.execute('DELETE FROM game_info '
                           'WHERE userid = %s OR opponentid = %s;', (userid, userid))
        elif userid == result[0]:
            cursor.execute('UPDATE game_info SET userid = -1 '
                       'WHERE userid = %s', (userid, ))
        else:
            cursor.execute('UPDATE game_info SET opponentid = -1 '
                       'WHERE opponentid = %s', (userid, ))
    response['status'] = 'cleared'

    return JsonResponse(response)


def checkwin(request):
    """Implements path('checkwin/', ingame.checkstatus, name='checkstatus').
    """
    if request.method != 'POST':
        return HttpResponse(status=404)

    userid = request.POST.get('userid')
    response = {}

    cursor = connection.cursor()
    cursor.execute('SELECT game_status, piece_cnt FROM game_info '
                   'WHERE userid = %s;', (userid, ))

    status, count = cursor.fetchone()
    response = {}
    if status == "Win":
        response = {
            'isWin': True,
            'num_piece': count,
        }
    elif status in ["Lose", "OnGoing"]:
        response = {
            'isWin': False,
            'num_piece': count,
        }
    else:
        response = {
            'isWin': False,
            'num_piece': 0,
        }

    return JsonResponse(response)
