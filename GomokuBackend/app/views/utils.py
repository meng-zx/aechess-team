DEBUG = 1
LENGTH = 0.025


def Dprint(*args, **kwargs):
    if DEBUG == 1:
        print(*args, **kwargs)


def grid_to_coordinate(grid):
    return (grid[0]*LENGTH, 0, grid[1]*LENGTH)


def coordinate_to_grid(coordinate):
    return (int(coordinate[0]/LENGTH), int(coordinate[2]/LENGTH))
