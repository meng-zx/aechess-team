using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Networking;

using MyGlobal;

public class InGameController : MonoBehaviour
{
    public string server_ip_address = "18.218.77.102";
    public GameObject Chessboard;
    public TextMeshProUGUI Hint_Text_Box;
    public TextMeshProUGUI Tittle_Text_Box;
    public Button confirm_button;

    private Stage_Codes stage = Stage_Codes.wait_matching;
    float time;
    public float time_delay = 0.5f;
    bool delay = false;
    MarkerTrackingSystem side_markers_tracking = new MarkerTrackingSystem();

    // private HttpRequestHandler http_request_handler;
    private int userid;
    private bool prev_piece_flag = false;
    private Vector3 Prev_new_piece = new Vector3(0, 0, 0);
    private bool first_play_flag = true;
    private bool confirm_button_clicked = false;
    private bool exit_button_clicked = false;

    // public TextMeshProUGUI Debug_Text_Box;

    [SerializeField]
    GameObject black_piece_prefab;

    [SerializeField]
    GameObject white_piece_prefab;

    public GameObject chesspiece;

    private int piece_cnt;

    private bool new_piece_marker_traked = false;

    public GameObject[] Corners = new GameObject[4];


    // MockServer mock_server = new MockServer();


    waitformatch_json waitformatch_response = new waitformatch_json();
    bool waitformatch_request_done = true;

    sendpiece_json sendpiece_response = new sendpiece_json();
    bool sendpiece_request_done = true;

    checkstatus_json checkstatus_response = new checkstatus_json();
    bool checkstatus_request_done = true;

    endgame_json endgame_response = new endgame_json();
    bool endgame_request_done = true;

    Vector3 loc_on_chessboard = new Vector3();

    // Start is called before the first frame update
    void Start()
    {
        time = 0.0f;
        delay = true;
        stage = Stage_Codes.wait_matching;
        modify_title_text("Matching");
        modify_hint_text("Wait for game matching...");
        userid = GameObject.Find("StartInfo").GetComponent<keepData>().userid;
        // debug_text(userid.ToString());
        confirm_button.gameObject.SetActive(false);
        new_piece_marker_traked = false;
        exit_button_clicked = false;

        waitformatch_request_done = true;
        sendpiece_request_done = true;
        checkstatus_request_done = true;
        endgame_request_done = true;
         


        // confirm_button.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (time > time_delay || (!delay))
        {
            time = 0.0f;

            switch(stage){
                    case Stage_Codes.wait_matching:
                    // modify_hint_text(userid.ToString());
                    waitformatch_request_done = false;
                    StartCoroutine(waitformatch_request(userid));
                    stage = Stage_Codes.wait_matching_wait;
                    delay = false;

                    break;

                case Stage_Codes.wait_matching_wait:
                    if (!waitformatch_request_done){

                        // wait web request
                    }
                    else{
                        StopCoroutine(waitformatch_request(userid));
                        if (waitformatch_response.status == "matched")
                        {
                            first_play_flag = waitformatch_response.isFirst;
                            prev_piece_flag = !waitformatch_response.isFirst;
                            stage = Stage_Codes.find_markers;
                            delay = false;
                        }
                        else
                        {
                            stage = Stage_Codes.wait_matching;
                            delay = true;
                        }
                        if (exit_button_clicked){
                            stage = Stage_Codes.end_game;
                            delay = false;
                        }
                    }
                    break;
                
                case Stage_Codes.find_markers:
                    modify_title_text("Find Markers");
                    modify_hint_text("Please scan the markers around the corners");
                    delay = false;
                    if (side_markers_tracking.all_markers_tracked())
                    {
                        modify_hint_text("All markers found!");
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
                    if (exit_button_clicked){
                        stage = Stage_Codes.end_game;
                        delay = false;
                    }
                    break;
                case Stage_Codes.player_turn:
                
                    confirm_button.gameObject.SetActive(true);
                    modify_title_text("Your turn");
                    modify_hint_text("Put a new piece and then put a marker on it");

                    confirm_button.interactable = new_piece_in_range();
                    if (prev_piece_flag)
                    {
                        // has a previous piece to be renderred
                        add_piece(Prev_new_piece, !first_play_flag);

                        prev_piece_flag = false;
                    }

                    if (confirm_button_clicked){
                        loc_on_chessboard = transfer_to_chessboard_coordinate(
                            chesspiece.transform.position
                        );
                        sendpiece_request_done = false;
                        StartCoroutine(send_sendpiece_request(userid, loc_on_chessboard));
                        stage = Stage_Codes.player_turn_wait;
                        delay = false;
                    }
                    else {
                        stage = Stage_Codes.player_turn;
                        prev_piece_flag = false;
                        delay = false;
                        if (exit_button_clicked){
                            stage = Stage_Codes.end_game;
                            delay = false;
                        }

                    }
                    break;
                
                case Stage_Codes.player_turn_wait:
                    if (!sendpiece_request_done){
                        // wait
                    }
                    else{
                        StopCoroutine(send_sendpiece_request(userid, loc_on_chessboard));
                        confirm_button_clicked = false;
                        if (sendpiece_response.status == "end game")
                        {
                            end_scene();
                        }
                        stage = Stage_Codes.wait_opponent;
                        delay = true;
                        confirm_button.gameObject.SetActive(false);
                        if (exit_button_clicked){
                            stage = Stage_Codes.end_game;
                            delay = false;
                        }
                    }
                    break;
                case Stage_Codes.wait_opponent:
                    modify_title_text("Opponent's turn");
                    modify_hint_text("Please wait for opponent to put piece");
                    checkstatus_request_done = false;
                    StartCoroutine(send_checkstatus_request(userid));
                    stage = Stage_Codes.wait_opponent_wait;
                    delay = false;
                    break;
                case Stage_Codes.wait_opponent_wait:
                    if (!checkstatus_request_done){
                        //wait
                    }
                    else{
                        StopCoroutine(send_checkstatus_request(userid));
                        if (checkstatus_response.status == "player turn")
                        {
                            stage = Stage_Codes.player_turn;
                            prev_piece_flag = true;
                            Prev_new_piece = new Vector3(
                                checkstatus_response.new_piece_location[0],
                                checkstatus_response.new_piece_location[1],
                                checkstatus_response.new_piece_location[2]
                            );
                            delay = false;
                        }
                        else if (checkstatus_response.status == "end game")
                        {
                            // debug_text(checkstatus_response.status);
                            end_scene();
                        }
                        else if (checkstatus_response.status == "opponent turn")
                        {
                            stage = Stage_Codes.wait_opponent;
                            prev_piece_flag = false;
                            delay = true;
                        }
                        if (exit_button_clicked){
                            stage = Stage_Codes.end_game;
                            delay = false;
                        }
                    }
                    break;
                case Stage_Codes.end_game:
                    modify_hint_text("end game");
                    endgame_request_done = false;
                    StartCoroutine(send_endgame_request(userid));
                    stage = Stage_Codes.end_game_wait;
                    delay = false;
                    break;
                case Stage_Codes.end_game_wait:
                    if (!endgame_request_done){
                        //wait
                    }
                    else{
                        StopCoroutine(send_endgame_request(userid));
                        if (endgame_response.status == "ended")
                        {
                            end_scene();
                        }
                        else
                        {
                            // TODO raise error to error handler
                        }
                    }
                    break;




            }

            
        }
        time += 1.0f * Time.deltaTime;
    }

    public void exit_button_onClick()
    {
        // http_request_handler.send_endgame_request(userid);
        // end_scene();
        exit_button_clicked = true;
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
        wait_opponent_wait,

        end_game,
        end_game_wait
    }

    private void modify_hint_text(string s, int fontsize = 48)
    {
        Hint_Text_Box.text = s;
        Hint_Text_Box.fontSize = fontsize;
    }

    private void modify_title_text(string s, int fontsize = 60)
    {
        Tittle_Text_Box.text = s;
        Tittle_Text_Box.fontSize = fontsize;
    }

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


    IEnumerator waitformatch_request(int send_userid){
        // POST
        // string uri = "https://" + server_ip_address +  "/waitformatch/";
        string uri = "https://18.218.77.102/waitformatch/";
        // TODO: remove hard-defined rules
        WWWForm form = new WWWForm();
        form.AddField("userid", send_userid);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(uri, form))
        {
            webRequest.certificateHandler = new MyGlobal.ControllerHelper.BypassCertificate();

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                modify_hint_text("POST waitformatch request error: " + webRequest.error);
                // waitformatch_request_done = true;
            }
            else
            {
                // modify_hint_text("POST waitformatch request success!");
                waitformatch_response = JsonUtility.FromJson<waitformatch_json>(webRequest.downloadHandler.text);
                waitformatch_request_done = true; // Please put this line after putting the result into json class
            }
        }
    }

    IEnumerator  send_sendpiece_request(
            int send_userid,
            Vector3 send_pos
        )
    {
        // POST
        string uri = "https://18.218.77.102/sendpiece/";
        // TODO: remove hard-defined rules
        WWWForm form = new WWWForm();
        form.AddField("userid", send_userid);
        // modify_hint_text(send_pos.ToString()); //(-0.03, 0.02, 0.02)

        form.AddField("marker_location", send_pos.ToString());

        using (UnityWebRequest webRequest = UnityWebRequest.Post(uri, form))
        {
            webRequest.certificateHandler = new MyGlobal.ControllerHelper.BypassCertificate();

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                modify_hint_text("POST sendpiece request error: " + webRequest.error);
                // sendpiece_request_done = true;
            }
            else
            {
                // modify_hint_text("POST sendpiece request success!");
                sendpiece_response = JsonUtility.FromJson<sendpiece_json>(webRequest.downloadHandler.text);
                sendpiece_request_done = true; // Please put this line after putting the result into json class
            }
        }
    }

     IEnumerator send_checkstatus_request(int send_userid)
    {
        // POST
        string uri = "https://18.218.77.102/checkstatus/";
        // TODO: remove hard-defined rules
        WWWForm form = new WWWForm();
        form.AddField("userid", send_userid);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(uri, form))
        {
            webRequest.certificateHandler = new MyGlobal.ControllerHelper.BypassCertificate();

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                modify_hint_text("POST checkstatus request error: " + webRequest.error);
                // checkstatus_request_done = true;
            }
            else
            {
                // modify_hint_text("POST checkstatus request success!");
                checkstatus_response = JsonUtility.FromJson<checkstatus_json>(webRequest.downloadHandler.text);
                checkstatus_request_done = true; // Please put this line after putting the result into json class
            }
        }
    }

    IEnumerator send_endgame_request(int send_userid)
    {
        // POST
        string uri = "https://18.218.77.102/endgame/";
        // TODO: remove hard-defined rules
        WWWForm form = new WWWForm();
        form.AddField("userid", send_userid);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(uri, form))
        {
            webRequest.certificateHandler = new MyGlobal.ControllerHelper.BypassCertificate();

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                modify_hint_text("POST endgame request error: " + webRequest.error);
                // endgame_request_done = true;
            }
            else
            {
                // modify_hint_text("POST endgame request success!");
                endgame_response = JsonUtility.FromJson<endgame_json>(webRequest.downloadHandler.text);
                endgame_request_done = true; // Please put this line after putting the result into json class
            }
        }
    }


    [Serializable]
    public class waitformatch_json
    {
        public string status;
        public bool isFirst;

        public waitformatch_json()
        {
            status = "continue waiting";
            isFirst = true;
        }
    }

    [Serializable]
    public class sendpiece_json
    {
        public string status;

        public sendpiece_json()
        {
            status = "end game";
        }
    }

    [Serializable]
    public class checkstatus_json
    {
        public string status;
        public List<float> new_piece_location;

        public checkstatus_json()
        {
            status = "end game";
        }
    }

    [Serializable]
    public class endgame_json
    {
        public string status;

        public endgame_json()
        {
            status = "ended"; // todo
        }
    }

    // public class MockServer
    // {
    //     private string ip_address;
    //     private Dictionary<int, int> waitformatch_dict = new Dictionary<int, int>();
    //     private Dictionary<int, int> checkstatus_dict = new Dictionary<int, int>();

    //     public waitformatch_json wait_for_match_request(int send_userid)
    //     {
    //         waitformatch_json waitformatch_result = new waitformatch_json();
    //         if (waitformatch_dict.ContainsKey(send_userid))
    //         {
    //             if (waitformatch_dict[send_userid] > 0)
    //             {
    //                 waitformatch_dict[send_userid] -= 1;
    //             }
    //             else
    //             {
    //                 waitformatch_dict[send_userid] = 5;
    //                 waitformatch_result.status = "matched";
    //                 waitformatch_result.isFirst = true;
    //             }
    //         }
    //         else
    //         {
    //             waitformatch_dict[send_userid] = 5;
    //         }
    //         return waitformatch_result;
    //     }

    //     public sendpiece_json send_pos_request(int send_userid, Vector3 pos)
    //     {
    //         sendpiece_json result = new sendpiece_json();
    //         result.status = "opponent turn";
    //         return result;
    //     }

    //     public checkstatus_json check_status_request(int send_userid)
    //     {
    //         checkstatus_json result = new checkstatus_json();
    //         if (checkstatus_dict.ContainsKey(send_userid))
    //         {
    //             if (checkstatus_dict[send_userid] > 0)
    //             {
    //                 result.status = "player turn";
    //                 List<float> new_loc = new List<float>()
    //                 {
    //                     0.05f - 0.025f * checkstatus_dict[send_userid],
    //                     0.0f,
    //                     0.0f
    //                 };
    //                 result.new_piece_location = new_loc;
    //                 checkstatus_dict[send_userid] -= 1;
    //             }
    //             else
    //             {
    //                 result.status = "end game";
    //                 List<float> new_loc = new List<float>() { 0.0f, 0.0f, 0.0f };
    //                 result.new_piece_location = new_loc;
    //                 checkstatus_dict[send_userid] = 4;
    //             }
    //         }
    //         else
    //         {
    //             checkstatus_dict[send_userid] = 4;

    //             result.status = "player turn";
    //             List<float> new_loc = new List<float>()
    //             {
    //                 0.05f - 0.025f * checkstatus_dict[send_userid],
    //                 0.0f,
    //                 0.0f
    //             };
    //             result.new_piece_location = new_loc;
    //             checkstatus_dict[send_userid] -= 1;
    //         }
    //         return result;
    //     }

    //     public endgame_json mock_end_game(int send_userid)
    //     {
    //         endgame_json result = new endgame_json();
    //         return result;
    //     }
    // }
}
