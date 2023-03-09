using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class InGameController : MonoBehaviour
{
    public string server_ip_address = "0.0.0.0";
    public GameObject Chessboard;
    public TextMeshProUGUI Hint_Text_Box;
    public Button confirm_button;

    private Stage_Codes stage = Stage_Codes.wait_matching;
    float time;
    public float time_delay = 0.5f;
    bool delay = false;
    MarkerTrackingSystem side_markers_tracking = new MarkerTrackingSystem();

    private HttpRequestHandler http_request_handler;
    private int userid;
    private int gameid;
    private bool prev_piece_flag = false;
    private Vector3 Prev_new_piece = new Vector3(0, 0, 0);
    private bool first_play_flag = true;
    private bool confirm_button_clicked = false;

    // public TextMeshProUGUI Debug_Text_Box;

    [SerializeField]
    GameObject black_piece_prefab;

    [SerializeField]
    GameObject white_piece_prefab;

    public GameObject chesspiece;

    private int piece_cnt;

    private bool new_piece_marker_traked = false;

    public GameObject[] Corners = new GameObject[4];

    // Start is called before the first frame update
    void Start()
    {
        time = 0.0f;
        delay = true;
        stage = Stage_Codes.wait_matching;
        modify_hint_text("Wait for game matching...");
        http_request_handler = new HttpRequestHandler(server_ip_address);
        userid = GameObject.Find("StartInfo").GetComponent<keepData>().userid;
        // debug_text(userid.ToString());
        confirm_button.gameObject.SetActive(false);
        new_piece_marker_traked = false;

        // confirm_button.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (time > time_delay || (!delay))
        {
            time = 0.0f;
            if (stage == Stage_Codes.wait_matching)
            {
                modify_hint_text("Wait for game matching...");

                waitformatch_json waitformatch_response =
                    http_request_handler.send_waitformatch_request(userid);

                if (waitformatch_response.status == "matched")
                {
                    first_play_flag = waitformatch_response.isFirst;
                    prev_piece_flag = !waitformatch_response.isFirst;
                    gameid = waitformatch_response.gameid;
                    stage = Stage_Codes.find_markers;
                    delay = false;
                }
                else
                {
                    stage = Stage_Codes.wait_matching;
                    delay = true;
                }
            }
            else if (stage == Stage_Codes.find_markers)
            {
                modify_hint_text("Find markers");
                delay = false;
                if (side_markers_tracking.all_markers_tracked())
                {
                    modify_hint_text("All markers found");
                    if (first_play_flag)
                    {
                        stage = Stage_Codes.player_turn;
                        delay = false;
                    }
                    else
                    {
                        stage = Stage_Codes.wait_opponent;
                        delay = true;
                    }
                }
            }
            else if (stage == Stage_Codes.player_turn)
            {
                confirm_button.gameObject.SetActive(true);
                modify_hint_text("Your turn");

                confirm_button.interactable = new_piece_in_range();

                if (prev_piece_flag)
                {
                    add_piece(Prev_new_piece, !first_play_flag);

                    prev_piece_flag = false;
                }

                if (confirm_button_clicked)
                {
                    confirm_button_clicked = false;
                    Vector3 loc_on_chessboard = transfer_to_chessboard_coordinate(
                        chesspiece.transform.position
                    );

                    // add_piece(loc_on_chessboard, first_play_flag);

                    sendpiece_json sendpiece_response = http_request_handler.send_sendpiece_request(
                        userid,
                        gameid,
                        loc_on_chessboard
                    );

                    if (sendpiece_response.status == "end game")
                    {
                        end_scene();
                    }
                    stage = Stage_Codes.wait_opponent;
                    delay = true;
                    confirm_button.gameObject.SetActive(false);
                }
                else
                {
                    stage = Stage_Codes.player_turn;
                    prev_piece_flag = false;
                    delay = false;
                }
            }
            else if (stage == Stage_Codes.wait_opponent)
            {
                modify_hint_text("wait for opponent's turn");

                checkstatus_json check_status_response = new checkstatus_json();
                // check_status_response.status = "player turn";
                check_status_response = http_request_handler.send_checkstatus_request(
                    userid,
                    gameid
                );
                if (check_status_response.status == "player turn")
                {
                    stage = Stage_Codes.player_turn;
                    prev_piece_flag = true;
                    Prev_new_piece = new Vector3(
                        check_status_response.new_piece_location[0],
                        check_status_response.new_piece_location[1],
                        check_status_response.new_piece_location[2]
                    );
                    delay = false;
                }
                else if (check_status_response.status == "end game")
                {
                    // debug_text(check_status_response.status);
                    end_scene();
                }
                else if (check_status_response.status == "opponent turn")
                {
                    stage = Stage_Codes.wait_opponent;
                    prev_piece_flag = false;
                    delay = true;
                }
            }
        }
        time += 1.0f * Time.deltaTime;
    }

    public void exit_button_onClick()
    {
        http_request_handler.send_endgame_request(userid);
        end_scene();
    }

    private void end_scene()
    {
        GameObject.Find("userInfo").GetComponent<keepData>().userid = userid;
        SceneManager.LoadScene("Scenes/EndGame");
    }

    enum Stage_Codes
    {
        wait_matching,
        wait_matching_wait,
        find_markers,
        player_turn,

        player_turn_wait,
        wait_opponent,
        wait_opponent_wait
    }

    private void modify_hint_text(string s, int fontsize = 48)
    {
        Hint_Text_Box.text = s;
        Hint_Text_Box.fontSize = fontsize;
    }

    // private void debug_text(string s, int fontsize = 48)
    // {
    //     Debug_Text_Box.text = s;
    //     Debug_Text_Box.fontSize = fontsize;
    // }

    private bool new_piece_in_range()
    {
        // PNPoly Algorithm
        // More info at: https://wrfranklin.org/Research/Short_Notes/pnpoly.html

        if (!new_piece_marker_traked)
        {
            return false;
        }
        int nvert = 4;
        Vector3 piece_loc_on_chessboard = transfer_to_chessboard_coordinate(
            chesspiece.transform.position
        );
        float testx = piece_loc_on_chessboard.x;
        float testy = piece_loc_on_chessboard.z;

        float[] vertx = new float[nvert];
        float[] verty = new float[nvert];

        for (int n = 0; n < nvert; n++)
        {
            Vector3 loc = transfer_to_chessboard_coordinate(Corners[n].transform.position);
            vertx[n] = loc.x;
            verty[n] = loc.z;
        }

        int i = 0;
        int j = 0;
        bool c = false;

        for (i = 0, j = nvert - 1; i < nvert; j = i++)
        {
            if (
                ((verty[i] > testy) != (verty[j] > testy))
                && (
                    testx
                    < (vertx[j] - vertx[i]) * (testy - verty[i]) / (verty[j] - verty[i]) + vertx[i]
                )
            )
                c = !c;
        }
        return c;
    }

    public void confirm_button_OnClick()
    {
        confirm_button_clicked = true;
    }

    public void marker_1_tracked()
    {
        side_markers_tracking.side_marker_1_tracked = true;
    }

    public void marker_2_tracked()
    {
        side_markers_tracking.side_marker_2_tracked = true;
    }

    public void marker_4_tracked()
    {
        side_markers_tracking.side_marker_4_tracked = true;
    }

    public void new_piece_target_tracked()
    {
        new_piece_marker_traked = true;
    }

    public class MarkerTrackingSystem
    {
        public bool side_marker_1_tracked;
        public bool side_marker_2_tracked;
        public bool side_marker_4_tracked;

        public MarkerTrackingSystem()
        {
            side_marker_1_tracked = false;
            side_marker_2_tracked = false;
            side_marker_4_tracked = false;
        }

        public bool all_markers_tracked()
        {
            return side_marker_1_tracked && side_marker_2_tracked && side_marker_4_tracked;
        }
    }

    private Vector3 transfer_to_chessboard_coordinate(Vector3 world_pos)
    {
        return Chessboard.transform.InverseTransformPoint(world_pos);
    }

    public void add_piece(Vector3 chessboard_coordinate_pos, bool isBlack = true)
    {
        GameObject new_piece = new GameObject();
        if (isBlack)
        {
            new_piece = GameObject.Instantiate(black_piece_prefab, Chessboard.transform);
        }
        else
        {
            new_piece = GameObject.Instantiate(white_piece_prefab, Chessboard.transform);
        }

        new_piece.name = "ChessPiece" + piece_cnt.ToString();
        new_piece.transform.localPosition = chessboard_coordinate_pos;
        new_piece.transform.rotation = Chessboard.transform.rotation;
        piece_cnt += 1;

        return;
    }

    public class HttpRequestHandler
    {
        private MockServer mock_server = new MockServer();
        private string ip_address;

        public HttpRequestHandler(string ip)
        {
            ip_address = ip;
        }

        public waitformatch_json send_waitformatch_request(int send_userid)
        {
            waitformatch_json waitformatch_result = new waitformatch_json();

            // TODO: Send request to real server and phase responded json file to c# class.
            waitformatch_result = mock_server.wait_for_match_request(send_userid);

            return waitformatch_result;
        }

        public sendpiece_json send_sendpiece_request(
            int send_userid,
            int send_gameid,
            Vector3 send_pos
        )
        {
            sendpiece_json result = new sendpiece_json();

            // TODO: change Vector3 pos to List<float>

            // TODO: Send request to real server and phase responded json to c# class.
            result = mock_server.send_pos_request(send_userid, send_gameid, send_pos);
            return result;
        }

        public checkstatus_json send_checkstatus_request(int send_userid, int send_gameid)
        {
            checkstatus_json result = new checkstatus_json();

            // TODO: Send request to real server and phase responded json to c# class.
            result = mock_server.check_status_request(send_userid, send_gameid);

            return result;
        }

        public endgame_json send_endgame_request(int send_userid)
        {
            endgame_json endgame_result = new endgame_json();

            // TODO: Send request to real server and phase responded json file to c# class.
            endgame_result = mock_server.mock_end_game(send_userid);

            return endgame_result;
        }
    }

    public class waitformatch_json
    {
        public string status;
        public bool isFirst;
        public int gameid;

        public waitformatch_json()
        {
            status = "continue waiting";
            isFirst = true;
            gameid = -1;
        }
    }

    public class sendpiece_json
    {
        public string status;

        public sendpiece_json()
        {
            status = "end game";
        }
    }

    public class checkstatus_json
    {
        public string status;
        public List<float> new_piece_location;

        public checkstatus_json()
        {
            status = "end game";
        }
    }

    public class endgame_json
    {
        public string status;

        public endgame_json()
        {
            status = "ended";
        }
    }

    public class MockServer
    {
        private string ip_address;
        private Dictionary<int, int> waitformatch_dict = new Dictionary<int, int>();
        private Dictionary<int, int> checkstatus_dict = new Dictionary<int, int>();

        public waitformatch_json wait_for_match_request(int send_userid)
        {
            waitformatch_json waitformatch_result = new waitformatch_json();
            if (waitformatch_dict.ContainsKey(send_userid))
            {
                if (waitformatch_dict[send_userid] > 0)
                {
                    waitformatch_dict[send_userid] -= 1;
                }
                else
                {
                    waitformatch_dict[send_userid] = 5;
                    waitformatch_result.status = "matched";
                    waitformatch_result.isFirst = true;
                    waitformatch_result.gameid = 1;
                }
            }
            else
            {
                waitformatch_dict[send_userid] = 5;
            }
            return waitformatch_result;
        }

        public sendpiece_json send_pos_request(int send_userid, int send_gameid, Vector3 pos)
        {
            sendpiece_json result = new sendpiece_json();
            result.status = "opponent turn";
            return result;
        }

        public checkstatus_json check_status_request(int send_userid, int send_gameid)
        {
            checkstatus_json result = new checkstatus_json();
            if (checkstatus_dict.ContainsKey(send_userid))
            {
                if (checkstatus_dict[send_userid] > 0)
                {
                    result.status = "player turn";
                    List<float> new_loc = new List<float>()
                    {
                        0.05f - 0.025f * checkstatus_dict[send_userid],
                        0.0f,
                        0.0f
                    };
                    result.new_piece_location = new_loc;
                    checkstatus_dict[send_userid] -= 1;
                }
                else
                {
                    result.status = "end game";
                    List<float> new_loc = new List<float>() { 0.0f, 0.0f, 0.0f };
                    result.new_piece_location = new_loc;
                    checkstatus_dict[send_userid] = 4;
                }
            }
            else
            {
                checkstatus_dict[send_userid] = 4;

                result.status = "player turn";
                List<float> new_loc = new List<float>()
                {
                    0.05f - 0.025f * checkstatus_dict[send_userid],
                    0.0f,
                    0.0f
                };
                result.new_piece_location = new_loc;
                checkstatus_dict[send_userid] -= 1;
            }
            return result;
        }

        public endgame_json mock_end_game(int send_userid)
        {
            endgame_json result = new endgame_json();
            return result;
        }
    }
}
