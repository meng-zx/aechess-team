DEBUG = 1
LENGTH = 0.025


def Dprint(*args, **kwargs):
    if DEBUG == 1:
        print(*args, **kwargs)


def grid_to_coordinate(grid):
    return (grid[0]*LENGTH, 0, grid[1]*LENGTH)


def coordinate_to_grid(coordinate):
    return (int(coordinate[0]/LENGTH), int(coordinate[2]/LENGTH))

def check_result(ruleid, all_piece, new_piece):
    """Check if game finished. Return true on finished games"""
    win_count = 5
    if ruleid == 1:
        win_count = 4
    if ruleid == 2:
        win_count = 6
    current_count = 0
    # up and down
    for i in range((1 - win_count), (win_count - 1), 1):
        if (new_piece[0], new_piece[1]+i) in all_piece:
            current_count += 1
            if current_count == win_count:
                return True
        else:
            current_count = 0
    # left and right:
    current_count = 0
    for i in range((1 - win_count), (win_count - 1), 1):
        if (new_piece[0]+i, new_piece[1]) in all_piece:
            current_count += 1
            if current_count == win_count:
                return True
        else:
            current_count = 0
    # y = x
    current_count = 0
    for i in range((1 - win_count), (win_count - 1), 1):
        if (new_piece[0]+i, new_piece[1]+i) in all_piece:
            current_count += 1
            if current_count == win_count:
                return True
        else:
            current_count = 0
    # y = -x
    current_count = 0
    for i in range((1 - win_count), (win_count - 1), 1):
        if (new_piece[0]+i, new_piece[1]-i) in all_piece:
            current_count += 1
            if current_count == win_count:
                return True
        else:
            current_count = 0
    return False
