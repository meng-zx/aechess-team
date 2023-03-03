using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PreGameController : MonoBehaviour
{

    private int userid;
    private int ruleid;


    public string ip_address;

    public TextMeshProUGUI Hint_Text_Box;

    private HttpRequestHandler http_request_handler;

    // Start is called before the first frame update
    void Start()
    {
        userid = -1;
        ruleid = 1;
        http_request_handler = new HttpRequestHandler(ip_address);
        modify_hint_text("ruleid: "+ ruleid);
    }
    public void start_button_onClick(){
        gamestart_json gamestart_response = new gamestart_json();
        userid = gamestart_response.userid;
        modify_hint_text("userid: "+ userid);
        GameObject.Find("StartInfo").GetComponent<keepData>().userid=userid;
        SceneManager.LoadScene("Scenes/InGame");

    }

    public class HttpRequestHandler {
        private string ip_address;

        private MockServer mock_server = new MockServer();

        public HttpRequestHandler(string ip)
        {
            ip_address = ip;
        }

        public gamestart_json send_gamestart_request(int send_ruleid){
            gamestart_json result = new gamestart_json();

            // TODO: Send request to real server and phase responded json to c# class.
            result = mock_server.check_gamestart_request(send_ruleid);
            

            return result;

        }
    }

    private void modify_hint_text(string s, int fontsize = 48)
    {
        Hint_Text_Box.text = s;
        Hint_Text_Box.fontSize = fontsize;
    }

    public class gamestart_json{
        public int userid;
        public gamestart_json(){
            userid =0;
        }
    }

    public class MockServer{
        public gamestart_json check_gamestart_request(int send_ruleid){
            gamestart_json result = new gamestart_json();
            result.userid =2;
            return result;
        }
    }

    
}
