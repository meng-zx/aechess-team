using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MyGlobal
{
    public class ControllerHelper
    {
        public class BypassCertificate : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                //Simply return true no matter what
                return true;
            }
        } 

        public static List<float> Vector3ToList(Vector3 vector)
        {
            List<float> floatList = new List<float>{
                vector.x,
                vector.y,
                vector.z
            };
            return floatList;
        }
        
        public string ip_address; // TODO

    }
}
