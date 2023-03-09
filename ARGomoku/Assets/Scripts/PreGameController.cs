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

    bool test_button_clicked = false;
    bool reset_button_clicked = false;

    bool get_webpage_done= true;

    string get_webpage_response_text;

    private Stage_Codes stage = Stage_Codes.do_nothing;

    gamestart_json gamestart_response = new gamestart_json();
    bool gamestart_request_done = true;

    private MockServer mock_server = new MockServer();

    // Start is called before the first frame update
    void Start()
    {
        userid = -1;
        ruleid = 1;
        http_request_handler = new HttpRequestHandler(ip_address);
        modify_hint_text("Welcome to AR Gomoku");
        start_button_clicked = false;
        test_button_clicked = false;
        reset_button_clicked = false;
        stage = Stage_Codes.do_nothing;
        get_webpage_done= true;
        gamestart_request_done = true;
        
    }

    void Update(){
        switch(stage){
            case Stage_Codes.do_nothing:
                if (start_button_clicked){
                    stage = Stage_Codes.start_game;
                    modify_hint_text("start button clicked");
                }
                else if (reset_button_clicked){
                    stage = Stage_Codes.reset_text;
                }
                else if (test_button_clicked){
                    stage = Stage_Codes.get_webpage;
                    
                }
                break;
            case Stage_Codes.get_webpage:
            // send get_page webrequest
                get_webpage_done = false;
                StartCoroutine(GetRequest("https://eecs388.org"));
                stage = Stage_Codes.get_webpage_wait;
                break;
            case Stage_Codes.get_webpage_wait:
                // Wait for webrequest to process

                if(!get_webpage_done){
                    //wait for webrequest to process
                }
                else{
                    // finished processing
                    StopCoroutine(GetRequest("https://eecs388.org"));

                    // do some game logic
                    modify_hint_text(get_webpage_response_text);
                    Debug.Log("stop");
                    test_button_clicked = false;
                    stage = Stage_Codes.do_nothing;

                }
                break;
            case Stage_Codes.reset_text:
                modify_hint_text("reset");
                reset_button_clicked = false;
                stage = Stage_Codes.do_nothing;
                break;
            case Stage_Codes.start_game:
                gamestart_request_done = false;
                StartCoroutine(gamestart_request(ruleid));
                stage = Stage_Codes.start_game_wait;
                break;
            case Stage_Codes.start_game_wait:
                if(!gamestart_request_done){
                    // wait
                }
                else{
                    StopCoroutine(gamestart_request(ruleid));
                    start_button_clicked = false;
                    userid = gamestart_response.userid;
                    modify_hint_text("userid: " + userid);
                    GameObject.Find("StartInfo").GetComponent<keepData>().userid = userid;
                    SceneManager.LoadScene("Scenes/InGame");
                    
                   
                }
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

        reset_button_clicked = true;
    }

    public void test_button_onClick()
    {

        test_button_clicked = true;
    }

    enum Stage_Codes
    {
        do_nothing,
        get_webpage,
        get_webpage_wait,

        start_game,
        start_game_wait,
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
    IEnumerator gamestart_request(int send_ruleid){
        // TODO: Send request to real server and phase responded json to c# class.
        gamestart_response = mock_server.check_gamestart_request(send_ruleid);
        gamestart_request_done = true;
        yield return null;

    
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
