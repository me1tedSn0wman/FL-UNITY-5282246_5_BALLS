using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(LineRenderer))]
public class Trajectory : MonoBehaviour
{
    [SerializeField] private float raycastMaxDist = 100f;
    [SerializeField] private float stepAwayFromWall = 0.15f;
    [SerializeField] private float stepUpWall = 0.15f;
    private LineRenderer line;

    private List<Vector3> points;

    public bool drawTrajectory;
    private Material lineMat;
    private float crntXPos=0;

    [SerializeField] private float animationSpeed = 0.2f;

    private void Awake() {
        line = GetComponent<LineRenderer>();
        lineMat = line.material;
    }

    private void Update()
    {

        if (GameManager.gameState == GameState.Gameplay) {
            UpdateLinePoints();
            AnimateLineMat();
        }

    }

    private Vector3 GetMousePos() {
        Vector3 pointerWorldPos = PlayerControl.Instance.GetMouseWorldPos();
        return new Vector3(pointerWorldPos.x, pointerWorldPos.y, 0);
    }

    private void UpdateLinePoints() {
        points = new List<Vector3>();
        Vector3 mousePos = GetMousePos();
        Vector3 startPos = GameplayManager.START_ANCHOR.position;

        
        points.Add(startPos);


        Vector3 tCurPos = startPos;
        Vector3 dir = mousePos - startPos;
        int layerMask = (1 << 8) + (1 << 7) + (1 <<11) + (1 << 12);
        layerMask = ~layerMask;

        for(int i=0; i<10; i++) {
            
            RaycastHit2D hit = Physics2D.Raycast(tCurPos, dir, raycastMaxDist, layerMask);

            if (hit.collider != null)
            {
                tCurPos = hit.point;

                if (hit.collider.CompareTag("Wall"))
                {
                    dir = new Vector3(-dir.x, dir.y);

                    if (tCurPos.x > 0) {
                        tCurPos = new Vector3(tCurPos.x - stepAwayFromWall, tCurPos.y + stepUpWall, tCurPos.z);
                    }else if (tCurPos.x <= 0)
                    {
                        tCurPos = new Vector3(tCurPos.x + stepAwayFromWall, tCurPos.y + stepUpWall, tCurPos.z);
                    }
                    points.Add(tCurPos);
                }
                else if (hit.collider.CompareTag("Ball") || hit.collider.CompareTag("EndWall")) {
                    points.Add(tCurPos);
                    break;
                }

            }
            else {
                break;
            }
        }


        DrawLine();
    }

    private void AnimateLineMat() {
        crntXPos = (crntXPos + Time.deltaTime * animationSpeed)%1f;
        lineMat.mainTextureOffset = new Vector2(crntXPos, 0);
    }


    private void DrawLine() { 
        line.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++) {
            line.SetPosition(i, points[i]);
        }
    }

    public void SetLineColor(Color color) {
        line.startColor = color;
        line.endColor = color;
    }
}
