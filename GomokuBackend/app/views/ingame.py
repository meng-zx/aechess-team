from datetime import datetime, timezone
from django.shortcuts import render
from django.http import JsonResponse, HttpResponse
from django.db import connection, transaction
from django.views.decorators.csrf import csrf_exempt
from urllib.parse import unquote
from app.views.utils import Dprint, grid_to_coordinate, coordinate_to_grid, check_result
import json


@csrf_exempt
@transaction.atomic
def waitformatch(request):
    if request.method != 'POST':
        return HttpResponse(status=404)

    userid = request.POST.get('userid')
    Dprint(userid)
    response = {}

    cursor = connection.cursor()

    # update last response time
    now = datetime.now(timezone.utc)
    cursor.execute('UPDATE match_queue SET TIME = %s '
                   'WHERE userid = %s;', (now.strftime("%Y-%m-%d %H:%M:%S"), userid))

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

    Dprint(response)
    return JsonResponse(response)


@csrf_exempt
@transaction.atomic
def checkstatus(request):
    """Implements path('checkstatus/', ingame.checkstatus, name='checkstatus').
    """
    if request.method != 'POST':
        return HttpResponse(status=404)

    userid = request.POST.get('userid')
    Dprint(userid)
    response = {}

    cursor = connection.cursor()
    cursor.execute('SELECT opponentid, game_status, player_turn, piece_cnt FROM game_info '
                   'WHERE userid = %s;', (userid, ))

    opponentid, status, turn, count= cursor.fetchone()
    response = {}
    if status in ["Win", "Lose"]:
        response = {
            'status': "end game",
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
            # if count > 0:
            cursor.execute('SELECT x, z FROM new_piece_info '
                'WHERE userid = %s;', (opponentid, ))
            new_piece = cursor.fetchone()
            # TODO: what if not found?
            new_piece = [int(s) for s in new_piece]
            response = {
                'status': "player turn",
                'new_piece_location': grid_to_coordinate(new_piece),
            }
            # else:
            #     # TODO: return 0,0,0?
            #     response = {
            #         'status': "player turn",
            #         'new_piece_location': [0, 0, 0],
            #     }


    # TODO: if not found, return empty response?
    Dprint(response)
    return JsonResponse(response)


@csrf_exempt
@transaction.atomic
def endgame(request):
    """Implements path('endgame/', ingame.endgame, name='endgame').
    """
    if request.method != 'POST':
        return HttpResponse(status=404)

    userid = request.POST.get('userid')
    Dprint(userid)

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
@transaction.atomic
def sendpiece(request):
    """Implements path('sendpiece/', ingame.sendpiece, name='sendpiece').
    """
    if request.method != 'POST':
        return HttpResponse(status=404)
    userid = request.POST.get('userid')
    # TODO: modify marker_location
    marker_location = request.POST.get('marker_location') # example: '(0.04%2c%20-0.08%2c%200.05)'
    Dprint(userid, marker_location)
    # example: '(0.04, -0.08, 0.05)'
    marker_location = unquote(marker_location)[1:-1].split(',')
    marker_location = [float(x) for x in marker_location]

    #Transform into grid
    grid_location = coordinate_to_grid(marker_location)
    response = {}


    cursor = connection.cursor()
    ## TODO: add error check for invalid gameid/userid?
    cursor.execute('SELECT game_status, opponentid, ruleid FROM game_info '
                    'WHERE userid = %s;', (userid, ))
    status, opponentid, ruleid = cursor.fetchone()
    print(status)
    if status in ["Win", "Lose"]:
        response = {
            'status': "end game"
        }
        return JsonResponse(response)
    if len(marker_location) == 3:
        # check if already exist
        cursor.execute('SELECT x, z FROM NEW_PIECE_INFO '
                        'WHERE userid = %s;', (userid, ))
        exist = cursor.fetchall()
        if (len(exist)==0):
            cursor.execute('INSERT INTO NEW_PIECE_INFO (userid, x, z) VALUES (%s, %s, %s);'
                        , (userid, grid_location[0], grid_location[1]))
        else:
            cursor.execute('UPDATE NEW_PIECE_INFO SET x = %s, z = %s'
                    'WHERE userid = %s;', (grid_location[0], grid_location[1],userid))
        ## TODO: add check for duplicate location?

        ## TODO: MVP: implement checking end of game and update game status
        
        # Update piece info
        cursor.execute('INSERT INTO ALL_PIECE_INFO (userid, x, z) VALUES'
                    '(%s, %s, %s)', (userid, grid_location[0], grid_location[1]))
        cursor.execute('UPDATE GAME_INFO SET PIECE_CNT = PIECE_CNT + 1'
                    'WHERE userid = %s;', (userid, ))
        cursor.execute('UPDATE GAME_INFO SET PLAYER_TURN = %s '
                    'WHERE userid = %s;', (False, userid))
        cursor.execute('UPDATE GAME_INFO SET PLAYER_TURN = %s '
                    'WHERE userid = %s;', (True, opponentid)) 
        ## TODO Acquire all chess piecees
        cursor.execute('SELECT x, z FROM ALL_PIECE_INFO '
                        'WHERE userid = %s;', (userid, ))
        all_piece = cursor.fetchall()
        ## TODO check if finished 
        game_result = check_result(ruleid, all_piece, grid_location)
        if game_result:
            response = {
                'status': "end game"
            }
            cursor.execute('UPDATE GAME_INFO SET GAME_STATUS = %s '
                    'WHERE userid = %s;', ("Win", userid)) 
            cursor.execute('UPDATE GAME_INFO SET GAME_STATUS = %s '
                    'WHERE userid = %s;', ("Lose", opponentid)) 
        else:
            response = {
                'status': "opponent turn"
            }
        print(all_piece)
        # Update piece info


    Dprint(response)
    return JsonResponse(response)
