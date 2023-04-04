from datetime import datetime, timedelta, timezone
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

    while True:
        cursor.execute('SELECT userid, TIME FROM match_queue '
                    'WHERE ruleid = %s AND matched = FALSE;', (ruleid, ))
        opponent = cursor.fetchone()
        if opponent is None:
            # opponent not found
            cursor.execute('INSERT INTO match_queue(ruleid, matched) VALUES (%s, FALSE);'
                        , (ruleid, ))
            cursor.execute('SELECT MAX(userid) FROM match_queue;')
            userid = cursor.fetchone()[0]
            break
        else:
            # opponent exists in database
            opponentid, response_time = opponent
            now = datetime.now(timezone.utc)
            Dprint(now - response_time)
            if now > response_time + timedelta(seconds=2):
                # if last response time is greater than 20 seconds, then delete this entry
                Dprint("invalud opponent")
                cursor.execute('DELETE FROM match_queue WHERE userid = %s;', (opponentid,))
            else:
                # opponent found
                Dprint("Opponent found")
                cursor.execute('INSERT INTO match_queue(ruleid, matched) VALUES (%s, TRUE);'
                            , (ruleid, ))
                cursor.execute('UPDATE match_queue SET matched = TRUE '
                            'WHERE userid = %s', (opponentid, ))

                cursor.execute('SELECT MAX(userid) FROM match_queue;')
                userid = cursor.fetchone()[0]
                cursor.execute('INSERT INTO game_info(userid, opponentid, ruleid, player_turn, game_status, piece_cnt) VALUES'
                            '(%s, %s, %s, %s, %s, %s);', (userid, opponentid, ruleid, 'TRUE', 'OnGoing', 0))
                cursor.execute('INSERT INTO game_info(userid, opponentid, ruleid, player_turn, game_status, piece_cnt) VALUES'
                        '(%s, %s, %s, %s, %s, %s);', (opponentid, userid, ruleid, 'FALSE', 'OnGoing', 0))
                break

    response['userid'] = userid

    Dprint(response)
    return JsonResponse(response)
