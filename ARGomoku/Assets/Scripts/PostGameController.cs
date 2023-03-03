using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PostGameController : MonoBehaviour
{
    private int userid;

    public string ip_address;

    public TextMeshProUGUI Hint_Text_Box;

    private HttpRequestHandler http_request_handler;


    // Start is called before the first frame update
    void Start()
    {
        userid= GameObject.Find("userInfo").GetComponent<keepData>().userid;
        http_request_handler = new HttpRequestHandler(ip_address);
        checkstats_json checkstats_response= new checkstats_json();
        checkstats_response = http_request_handler.send_checkstats_request(userid);
        string text_str = "You ";
        if (checkstats_response.isWin){
            text_str+="win \n in "+ checkstats_response.num_piece.ToString()+" move.";
        }
        else{
            text_str+="lose \n in "+ checkstats_response.num_piece.ToString()+" move.";
        }
        modify_hint_text(text_str);
    }

    public void return_button_onClick(){
        http_request_handler.send_clearrecords_request(userid);
        SceneManager.LoadScene("Scenes/StartGame");
    }

    private void modify_hint_text(string s, int fontsize = 48)
    {
        Hint_Text_Box.text = s;
        Hint_Text_Box.fontSize = fontsize;
    }

    public class HttpRequestHandler {
        private string ip_address;

        private MockServer mock_server = new MockServer();

        public HttpRequestHandler(string ip)
        {
            ip_address = ip;
        }

        public checkstats_json send_checkstats_request(int send_userid){
            checkstats_json result = new checkstats_json();

            // TODO: Send request to real server and phase responded json to c# class.
            result = mock_server.check_stats_request(send_userid);
            

            return result;

        }
        public clearrecords_json send_clearrecords_request(int send_userid){
            clearrecords_json result = new clearrecords_json();

            // TODO: Send request to real server and phase responded json to c# class.
            result = mock_server.mock_clear_records(send_userid);
            

            return result;

        }
    }

    public class checkstats_json{
        public bool isWin;
        public int num_piece;
        public checkstats_json(){
            isWin = true;
            num_piece = 0;
        }
    }

    public class clearrecords_json{
        public string status;
        public clearrecords_json(){
            status = "done";
        }
    }

    public class MockServer{
        public checkstats_json check_stats_request(int send_userid){
            checkstats_json result = new checkstats_json();
            result.isWin = false;
            result.num_piece = 5;
            return result;
        }

        public clearrecords_json mock_clear_records(int send_userid){
            clearrecords_json result = new clearrecords_json();
            return result;
        }
    }


}
