using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PostGameController : MonoBehaviour
{
    private int userid;

    public string ip_address;

    public TextMeshProUGUI Hint_Text_Box;

   

    MockServer mock_server = new MockServer();

    Stage_Codes stage = Stage_Codes.checkstats;
    bool return_button_cicked = false;

    checkstats_json checkstats_response = new checkstats_json();
    bool checkstats_request_done = true;

    clearrecords_json clearrecords_response = new clearrecords_json();

    bool clearrecords_request_done = true;


    // Start is called before the first frame update
    void Start()
    {
        userid= GameObject.Find("userInfo").GetComponent<keepData>().userid;
        stage = Stage_Codes.checkstats;
        return_button_cicked = false;

        checkstats_request_done = true;
        clearrecords_request_done = true;
    }

    void Update(){
        switch (stage){
            case Stage_Codes.checkstats:
                checkstats_request_done = false;
                StartCoroutine(send_checkstats_request(userid));
                stage = Stage_Codes.checkstats_wait;
                break;
            case Stage_Codes.checkstats_wait:
                if (!checkstats_request_done){
                    // wait
                }
                else{
                    StopCoroutine(send_checkstats_request(userid));
                    string text_str = "You ";
                    if (checkstats_response.isWin){
                        text_str+="win \n in "+ checkstats_response.num_piece.ToString()+" move.";
                    }
                    else{
                        text_str+="lose \n in "+ checkstats_response.num_piece.ToString()+" move.";
                    }
                    modify_hint_text(text_str);
                    stage = Stage_Codes.do_nothing;
                }
                break;
            case Stage_Codes.do_nothing:
                if (return_button_cicked){
                    stage = Stage_Codes.clearrecords;
                }
                break;
            case Stage_Codes.clearrecords:
                clearrecords_request_done = false;
                StartCoroutine(send_clearrecords_request(userid));
                stage = Stage_Codes.clearrecords_wait;
                break;
            case Stage_Codes.clearrecords_wait:
                if (!clearrecords_request_done){
                    //wait
                }
                else{
                    StopCoroutine(send_clearrecords_request(userid));
                    return_button_cicked = false;
                    SceneManager.LoadScene("Scenes/StartGame");
                }
                
                break;

        }

    }


    enum Stage_Codes
    {
        do_nothing,
        checkstats,
        checkstats_wait,

        clearrecords,
        clearrecords_wait
    }


    public void return_button_onClick(){
        return_button_cicked = true;
        // http_request_handler.send_clearrecords_request(userid);
        // SceneManager.LoadScene("Scenes/StartGame");
    }

    private void modify_hint_text(string s, int fontsize = 48)
    {
        Hint_Text_Box.text = s;
        Hint_Text_Box.fontSize = fontsize;
    }

    IEnumerator send_checkstats_request(int send_userid){
            checkstats_json result = new checkstats_json();

            // TODO: Send request to real server and phase responded json to c# class.
            yield return new WaitForSeconds(0.5f);
            checkstats_response = mock_server.check_stats_request(send_userid);
            checkstats_request_done = true; // Please put this line after putting the result into json class

        }

    IEnumerator send_clearrecords_request(int send_userid){
            

            // TODO: Send request to real server and phase responded json to c# class.
            yield return new WaitForSeconds(0.5f);
            clearrecords_response = mock_server.mock_clear_records(send_userid);
            clearrecords_request_done =true; // Please put this line after putting the result into json class

        }


    [Serializable]
    public class checkstats_json{
        public bool isWin;
        public int num_piece;
        public checkstats_json(){
            isWin = true;
            num_piece = 0;
        }
    }

   [Serializable]
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
