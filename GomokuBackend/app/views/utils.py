DEBUG = 1

def Dprint(*args, **kwargs):
    if DEBUG == 1:
        print(*args, **kwargs)
