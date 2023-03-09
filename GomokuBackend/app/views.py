from django.shortcuts import render
from django.http import JsonResponse, HttpResponse
from django.db import connection
from django.views.decorators.csrf import csrf_exempt
import json

def hello(request):
    if request.method != 'GET':
        return HttpResponse(status=404)

    # cursor = connection.cursor()
    # cursor.execute('SELECT username, message, time FROM chatts ORDER BY time DESC;')
    # rows = cursor.fetchall()

    cursor = connection.cursor()
    cursor.execute('SELECT * FROM MATCH_QUEUE;')
    rows1 = cursor.fetchall()

    cursor.execute('SELECT * FROM GAME_INFO;')
    rows2 = cursor.fetchall()

    cursor.execute('SELECT * FROM NEW_PIECE_INFO;')
    rows3 = cursor.fetchall()

    cursor.execute('SELECT * FROM ALL_PIECE_INFO;')
    rows4 = cursor.fetchall()

    response = {}
    response['MATCH_QUEUE'] = rows1
    response['GAME_INFO'] = rows2
    response['NEW_PIECE_INFO'] = rows3
    response['ALL_PIECE_INFO'] = rows4
    
    return JsonResponse(response)
