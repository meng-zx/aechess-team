using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Networking;

using MyGlobal;


public class PreGameController : MonoBehaviour
{
    private int userid;
    private int ruleid;

    public string ip_address;

    public TextMeshProUGUI Hint_Text_Box;



    bool start_button_clicked = false;

    bool test_button_clicked = false;
    bool reset_button_clicked = false;

    bool get_webpage_done= true;

    string get_webpage_response_text;

    private Stage_Codes stage = Stage_Codes.do_nothing;

    gamestart_json gamestart_response = new gamestart_json();
    bool gamestart_request_done = true;

    // Start is called before the first frame update
    void Start()
    {
        userid = -1;
        ruleid = 1;
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
                StartCoroutine(GetRequest());
                stage = Stage_Codes.get_webpage_wait;
                break;
            case Stage_Codes.get_webpage_wait:
                // Wait for webrequest to process

                if(!get_webpage_done){
                    //wait for webrequest to process
                }
                else{
                    // finished processing
                    StopCoroutine(GetRequest());

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
                modify_hint_text("start button clicked");
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

     IEnumerator GetRequest()
    {
        string uri = "https://18.218.77.102/hello/";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            webRequest.certificateHandler = new MyGlobal.ControllerHelper.BypassCertificate();

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
                     // TODO: jsonify the gotten result text
                     get_webpage_done = true; // Please put this line after getting the result
                    break;
            }
        }
    }
    IEnumerator gamestart_request(int send_ruleid){
        // POST
        string uri = "https://18.217.77.102/gamestart/";
        // TODO: remove hard-defined rules
        WWWForm form = new WWWForm();
        form.AddField("ruleid", send_ruleid);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(uri, form))
        {
            webRequest.certificateHandler = new MyGlobal.ControllerHelper.BypassCertificate();

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                modify_hint_text("POST gamestart request error: " + webRequest.error);
                gamestart_request_done = true;
            }
            else
            {
                modify_hint_text("POST gamestart request success!");
                gamestart_response = JsonUtility.FromJson<gamestart_json>(webRequest.downloadHandler.text);
                gamestart_request_done = true; // Please put this line after putting the result into json class
            }
        }


    
    }

    private void modify_hint_text(string s, int fontsize = 48)
    {
        Hint_Text_Box.text = s;
        Hint_Text_Box.fontSize = fontsize;
    }

    [Serializable]
    public class gamestart_json
    {
        public int userid;

        public gamestart_json()
        {
            userid = 0;
        }
    }
}
