"""routing URL Configuration

The `urlpatterns` list routes URLs to views. For more information please see:
    https://docs.djangoproject.com/en/4.1/topics/http/urls/
Examples:
Function views
    1. Add an import:  from my_app import views
    2. Add a URL to urlpatterns:  path('', views.home, name='home')
Class-based views
    1. Add an import:  from other_app.views import Home
    2. Add a URL to urlpatterns:  path('', Home.as_view(), name='home')
Including another URLconf
    1. Import the include() function: from django.urls import include, path
    2. Add a URL to urlpatterns:  path('blog/', include('blog.urls'))
"""
from django.contrib import admin
from django.urls import path
from app.views import hello, pregame, ingame, postgame

urlpatterns = [
    path('admin/', admin.site.urls),
    path('hello/', hello.hello, name='test'),
    # 1. Pre-Game
    path('gamestart/', pregame.gamestart, name='gamestart'),
    # 2. In-Game
    path('waitformatch/', ingame.waitformatch, name='waitformatch'),
    path('sendpiece/', ingame.sendpiece, name='sendpiece'),
    path('checkstatus/', ingame.checkstatus, name='checkstatus'),
    path('endgame/', ingame.endgame, name='endgame'),
    # 3. Post-Game
    path('clearrecords/', postgame.clearrecords, name='clearrecords'),
    path('checkwin/', postgame.checkwin, name='checkwin')
]
