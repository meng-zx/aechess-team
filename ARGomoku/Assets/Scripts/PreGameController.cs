using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Networking;

public class PreGameController : MonoBehaviour
{
    private int userid;
    private int ruleid;

    public string ip_address;

    public TextMeshProUGUI Hint_Text_Box;

    private HttpRequestHandler http_request_handler;

    bool start_button_clicked = false;
    bool reset_button_clicked = false;

    bool get_webpage_done= true;

    string get_webpage_response_text;

    private Stage_Codes stage = Stage_Codes.do_nothing;

    // Start is called before the first frame update
    void Start()
    {
        userid = -1;
        ruleid = 1;
        http_request_handler = new HttpRequestHandler(ip_address);
        modify_hint_text("Welcome to AR Gomoku");
        start_button_clicked = false;
        reset_button_clicked = false;
        stage = Stage_Codes.do_nothing;
        get_webpage_done= true;
    }

    void Update(){
        switch(stage){
            case Stage_Codes.do_nothing:
                if (reset_button_clicked){
                    stage = Stage_Codes.reset_text;
                }
                else if (start_button_clicked){
                    stage = Stage_Codes.get_webpage;
                    
                }
                break;
            case Stage_Codes.get_webpage:
            // send get_page webrequest
                get_webpage_done = false;
                StartCoroutine(GetRequest("http://neverssl.com"));
                stage = Stage_Codes.get_webpage_wait;
                break;
            case Stage_Codes.get_webpage_wait:
                // Wait for webrequest to process

                if(!get_webpage_done){
                    //wait for webrequest to process
                }
                else{
                    // finished processing
                    StopCoroutine(GetRequest("http://neverssl.com"));

                    // do some game logic
                    modify_hint_text(get_webpage_response_text);
                    Debug.Log("stop");
                    start_button_clicked = false;
                    stage = Stage_Codes.do_nothing;

                }
                break;
            case Stage_Codes.reset_text:
                modify_hint_text("reset");
                reset_button_clicked = false;
                stage = Stage_Codes.do_nothing;
                break;


        }
    }

    public void start_button_onClick()
    {
        // gamestart_json gamestart_response = new gamestart_json();
        // userid = gamestart_response.userid;
        // modify_hint_text("userid: " + userid);
        // GameObject.Find("StartInfo").GetComponent<keepData>().userid = userid;
        // SceneManager.LoadScene("Scenes/InGame");

        start_button_clicked = true;
    }

    public void reset_button_onClick()
    {
        // gamestart_json gamestart_response = new gamestart_json();
        // userid = gamestart_response.userid;
        // modify_hint_text("userid: " + userid);
        // GameObject.Find("StartInfo").GetComponent<keepData>().userid = userid;
        // SceneManager.LoadScene("Scenes/InGame");

        reset_button_clicked = true;
    }

    enum Stage_Codes
    {
        do_nothing,
        get_webpage,
        get_webpage_wait,
        reset_text
    }

     IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                     modify_hint_text(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                     modify_hint_text(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                     get_webpage_response_text = pages[page] + ":\nReceived: " + webRequest.downloadHandler.text;
                     get_webpage_done = true;
                    break;
            }
        }
    }


    public class HttpRequestHandler
    {
        private string ip_address;

        private MockServer mock_server = new MockServer();

        public HttpRequestHandler(string ip)
        {
            ip_address = ip;
        }

        public gamestart_json send_gamestart_request(int send_ruleid)
        {
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

    public class gamestart_json
    {
        public int userid;

        public gamestart_json()
        {
            userid = 0;
        }
    }

    public class MockServer
    {
        public gamestart_json check_gamestart_request(int send_ruleid)
        {
            gamestart_json result = new gamestart_json();
            result.userid = 2;
            return result;
        }
    }
}
