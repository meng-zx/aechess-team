from django.shortcuts import render
from django.http import JsonResponse, HttpResponse
from django.db import connection, transaction
from django.views.decorators.csrf import csrf_exempt
from app.views.utils import Dprint
import json


@csrf_exempt
@transaction.atomic
def gamestart(request):
    if request.method != 'POST':
        return HttpResponse(status=404)

    ruleid = request.POST.get('ruleid')
    Dprint(ruleid)
    response = {}

    cursor = connection.cursor()


    cursor.execute('SELECT userid FROM match_queue '
                   'WHERE ruleid = %s AND matched = FALSE;', (ruleid, ))
    opponentid = cursor.fetchone()

    if opponentid is None:
        cursor.execute('INSERT INTO match_queue(ruleid, matched) VALUES (%s, FALSE);'
                       , (ruleid, ))
        cursor.execute('SELECT MAX(userid) FROM match_queue;')
        userid = cursor.fetchone()[0]
    else:
        opponentid = opponentid[0]
        cursor.execute('INSERT INTO match_queue(ruleid, matched) VALUES (%s, TRUE);'
                       , (ruleid, ))
        cursor.execute('UPDATE match_queue SET matched = TRUE '
                       'WHERE userid = %s', (opponentid, ))

        cursor.execute('SELECT MAX(userid) FROM match_queue;')
        userid = cursor.fetchone()[0]
        cursor.execute('INSERT INTO game_info(userid, opponentid, player_turn, game_status, piece_cnt) VALUES'
                       '(%s, %s, %s, %s, %s);', (userid, opponentid, 'TRUE', 'OnGoing', 0))
        cursor.execute('INSERT INTO game_info(userid, opponentid, player_turn, game_status, piece_cnt) VALUES'
                '(%s, %s, %s, %s, %s);', (opponentid, userid, 'FALSE', 'OnGoing', 0))

    response['userid'] = userid

    Dprint(response)
    return JsonResponse(response)
