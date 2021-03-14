using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class laser : MonoBehaviour
{
    
    public float lineLength = 0.3f;
    public LineRenderer lineRenderer;
    private Vector3 boomPosOnPalette;
    private GameObject boom;
    private Renderer fishRd;
    private GameObject palette;
    private TextMesh textMesh;
    private int frame;
    private string text;
    private string currentKey;
   
    
    // Start is called before the first frame update
    void Start()
    {
        currentKey = "";
        text = "";
        frame = 0;
        lineRenderer = GetComponent<LineRenderer>();
        fishRd = GameObject.Find("fish").GetComponent<Renderer>();
        boom = GameObject.Find("Explosion");
        palette = GameObject.Find("PaletteTarget");
        textMesh = GameObject.Find("Text").GetComponent<TextMesh>();
    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 position = transform.position;
        Vector3 direction = transform.forward;
        RaycastHit hit;
        DrawLine(position, direction * lineLength + position);
 
        
        if (Physics.Raycast(transform.position, direction, out hit, lineLength))
        {
            
            boom.transform.position = hit.point;
            Matrix4x4 m1 = boom.transform.localToWorldMatrix;
            Matrix4x4 m2 = palette.transform.worldToLocalMatrix;
            boomPosOnPalette = (m2 * m1).MultiplyPoint3x4(boom.transform.position);
            float a = boomPosOnPalette[2];
            float b = boomPosOnPalette[0];
            
            if(b>= -0.0455f && b<=0.0455f) //when laser/boom is on the palette
            {
                Color fishColor=Color.Lerp(ChooseColor(a), Color.black, ChooseBrightness(b));
                fishRd.material.color = fishColor;
            }
            else if(b>=-0.397&&b<=-0.049) //when laser is on the keyboard
            {
                frame++;
               
                
                if (currentKey != ChooseKey(a, b)) frame = 0;

                if (frame == 200)
                {
                    if (ChooseKey(a,b)=="del") text=text.Remove(text.Length - 1,1);
                    else text += ChooseKey(a,b);

                }
                Debug.Log(frame);
                Debug.Log(ChooseKey(a, b));
                currentKey = ChooseKey(a, b);
                //when laser is on keyboard
            }
            //else 
            textMesh.text = text;
        }
        else
        {
            boom.transform.position = new Vector3(10, 10, 10);
        }
    }

    Color ChooseColor(float a)
    {
        if(a<=0.1f && a>=0.0645f){ return Color.red;}
        if (a<0.0645f && a>= 0.03f){return Color.yellow;}
        if (a<0.03f && a>=0){return Color.green;}
        if ( a<0 && a>= -0.032f){return Color.blue;}
        if (a<0.021f && a>=-0.064f){return Color.magenta;}
        if (a < 0.064f && a>=-1){return Color.black;}
        return Color.white;
    }
    
    void DrawLine(Vector3 start, Vector3 end)
    {

        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    float ChooseBrightness(float b) //returns a float ranging from [0,1]
    {
        return ((b + 0.0455f)/ (0.0455f * 2));
    }

    string ChooseKey(float a, float b)
    {
        b = b - 0.015f;
        if (a <= 0.0516 && a > 0.0176)
        {
            if(b >= -0.397 && b <= -0.3654) return "Q";
            if(b >= -0.3654 && b <= -0.333) return "W";
            if(b >= -0.333 && b <= -0.302) return "E";
            if(b >= -0.302 && b <= -0.272) return "R";
            if(b >= -0.272 && b <= -0.2397) return "T";
            if(b >= -0.2397 && b <= -0.2082) return "Y";
            if(b >= -0.2082 && b <= -0.1766) return "U";
            if(b >= -0.1766 && b <= -0.1442) return "I";
            if(b >= -0.1442 && b <= -0.1126) return "O";
            if(b >= -0.1126 && b <= -0.0081) return "P";
            return "";
        }
        if (a <= 0.0176 && a > -0.0162)
        {
            b = b - 0.011f;
            if(b >= -0.397 && b <= -0.3654) return "A";
            if(b >= -0.3654 && b <= -0.333) return "S";
            if(b >= -0.333 && b <= -0.302) return "D";
            if(b >= -0.302 && b <= -0.272) return "F";
            if(b >= -0.272 && b <= -0.2397) return "G";
            if(b >= -0.2397 && b <= -0.2082) return "H";
            if(b >= -0.2082 && b <= -0.1766) return "J";
            if(b >= -0.1766 && b <= -0.1442) return "K";
            if (b >= -0.1442 && b <= -0.1126) return "L";
            if (b >= -0.1126 && b <= -0.0583) return "del";
                return "";
        }
        if (a <= -0.0162 && a >= -0.0505)
        {
            b = b - 0.022f;
            if(b >= -0.397 && b <= -0.3654) return "Z";
            if(b >= -0.3654 && b <= -0.333) return "X";
            if(b >= -0.333 && b <= -0.302) return "C";
            if(b >= -0.302 && b <= -0.272) return "V";
            if(b >= -0.272 && b <= -0.2397) return "B";
            if(b >= -0.2397 && b <= -0.2082) return "N";
            if (b >= -0.2082 && b <= -0.1766) return "M";
            if (b >= -0.1766 && b <= -0.0750) return " ";
            return "";
        }

        return "";
    }
}
