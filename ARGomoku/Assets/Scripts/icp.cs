using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class icp : MonoBehaviour
{
    // Start is called before the first frame update
    // public int N = 3;   //Number of pioints to be aligned
    // public float min_err_dist = 0.001f;
    // public GameObject QR1;
    // public GameObject QR2;
    // public GameObject QR3;
    // public GameObject QR4;
    // public GameObject QR5;
    // public GameObject ref1;
    // public GameObject ref2;
    // public GameObject ref3;
    // public GameObject ref4;
    // public GameObject ref5;

    // public int update_frame_num = 10;
    // private int frame_cnt = 0;

    public Vector3 relative_pos_to_marker0 = new Vector3(0.084f, 0.0f, 0.117f);
    public GameObject[] QR_Markers = new GameObject[3];

    void Start() { }

    //private void print_Matrix_3x3(Matrix<double> m)
    //{
    //    Debug.Log("Start printing 3x3 matrix: ");
    //    Debug.Log(m[0, 0] + " " + m[0, 1] + " " + m[0, 2]);
    //    Debug.Log(m[1, 0] + " " + m[1, 1] + " " + m[1, 2]);
    //    Debug.Log(m[2, 0] + " " + m[2, 1] + " " + m[2, 2]);
    //}

    //private void print_Matrix_3x1(Matrix<double> m)
    //{
    //    Debug.Log("Start printing 3x1 matrix: ");
    //    Debug.Log("( " + m[0, 0] + ", " + m[1, 0] + ", " + m[2, 0] + " )'");
    //}



    // Update is called once per frame
    void Update()
    {
        Vector3[] markers = new Vector3[3];
        for (int i = 0; i < 3; i++)
        {
            markers[i] = QR_Markers[i].transform.position;
        }

        Vector3 x_dir = (markers[1] - markers[0]).normalized;
        Vector3 z_dir = (markers[2] - markers[0]).normalized;
        Vector3 y_dir = Vector3.Cross(z_dir, x_dir).normalized;
        Quaternion new_rotation = Quaternion.LookRotation(z_dir, y_dir);
        transform.localRotation = new_rotation;
        Vector3 new_pos =
            relative_pos_to_marker0[0] * x_dir
            + relative_pos_to_marker0[1] * y_dir
            + relative_pos_to_marker0[2] * z_dir
            + markers[0];
        transform.localPosition = new_pos;

        // Vector3[] origin = new Vector3[N];
        // Vector3[] target = new Vector3[N];

        // // experiment:
        // origin[0] = ref1.transform.position;
        // origin[1] = ref2.transform.position;
        // origin[2] = ref3.transform.position;
        // origin[3] = ref4.transform.position;
        // origin[4] = ref5.transform.position;

        // target[0] = QR1.transform.position;
        // target[1] = QR2.transform.position;
        // target[2] = QR3.transform.position;
        // target[3] = QR4.transform.position;
        // target[4] = QR5.transform.position;

        // float dist = 0;
        // for (int i = 0; i < N; i++)
        // {
        //     dist += Vector3.Distance(origin[i], target[i]) * Vector3.Distance(origin[i], target[i]);
        // }
        // dist /= N;
        // if (frame_cnt == 0)
        // {
        //     Matrix<double>[] origin_matrix_array = new Matrix<double>[N];
        //     Matrix<double>[] target_matrix_array = new Matrix<double>[N];

        //     // assign Vector3 to Matrix<double>
        //     for (int i = 0; i < N; i++)
        //     {
        //         // origin:
        //         origin_matrix_array[i] = Matrix<double>.Build.Dense(3, 1);
        //         origin_matrix_array[i][0, 0] = origin[i][0];
        //         origin_matrix_array[i][1, 0] = origin[i][1];
        //         origin_matrix_array[i][2, 0] = origin[i][2];

        //         // target:
        //         target_matrix_array[i] = Matrix<double>.Build.Dense(3, 1);
        //         target_matrix_array[i][0, 0] = target[i][0];
        //         target_matrix_array[i][1, 0] = target[i][1];
        //         target_matrix_array[i][2, 0] = target[i][2];
        //     }


        //     // Get center of mass:
        //     Matrix<double> origin_CoM = Matrix<double>.Build.Dense(3, 1);
        //     Matrix<double> target_CoM = Matrix<double>.Build.Dense(3, 1);

        //     for (int i = 0; i < N; i++)
        //     {
        //         origin_CoM += origin_matrix_array[i];
        //         target_CoM += target_matrix_array[i];
        //     }
        //     origin_CoM /= N;
        //     target_CoM /= N;



        //     // subtract center of mass
        //     Matrix<double>[] substracted_origin = new Matrix<double>[N];
        //     Matrix<double>[] substracted_target = new Matrix<double>[N];

        //     for (int i = 0; i < N; i++)
        //     {
        //         substracted_origin[i] = origin_matrix_array[i] - origin_CoM;
        //         substracted_target[i] = target_matrix_array[i] - target_CoM;
        //     }

        //     // Form cross-covarent matrix
        //     Matrix<double> W = Matrix<double>.Build.Dense(3, 3);

        //     for (int i = 0; i < N; i++)
        //     {
        //         W += substracted_target[i] * (substracted_origin[i].Transpose());
        //     }
        //     var svd = W.Svd();
        //     var R = svd.U * svd.VT;
        //     var T = target_CoM - R * origin_CoM;

        //     Vector3 C2 = new Vector3((float)R[0, 2], (float)R[1, 2], (float)R[2, 2]); //matrix4x4 getcolumn(2)
        //     Vector3 C1 = new Vector3((float)R[0, 1], (float)R[1, 1], (float)R[2, 1]); //matrix4x4 getcolumn(1)

        //     Quaternion additional_rotation = Quaternion.LookRotation(C2, C1);
        //     transform.localRotation = additional_rotation * transform.localRotation;
        //     transform.localPosition += new Vector3((float)T[0, 0], (float)T[1, 0], (float)T[2, 0]);


        // }
        // frame_cnt++;
        // if (frame_cnt == update_frame_num)
        // {
        //     frame_cnt = 0;
        // }
    }
}
