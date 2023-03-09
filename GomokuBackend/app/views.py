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

    response = {}
    response['message'] = 'hello from gomokubackend'
    return JsonResponse(response)
