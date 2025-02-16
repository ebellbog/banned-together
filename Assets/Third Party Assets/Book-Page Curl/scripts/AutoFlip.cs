using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(BookLive))]
public class AutoFlip : MonoBehaviour {
    public FlipMode Mode;
    public float PageFlipTime = 1;
    public float TimeBetweenPages = 1;
    public float DelayBeforeStarting = 0;
    public bool AutoStartFlip=true;
    public BookLive ControlledBook;
    public float flipSpeed = 5f;
    public float wiggleSpeed = 5f;
    public float wigglePercent = .05f;

    private bool isFlipping = false;
    private bool isWiggling = false;
    private Action afterWiggleHandler;

    // Use this for initialization
    void Start () {
        if (!ControlledBook)
            ControlledBook = GetComponent<BookLive>();
        if (AutoStartFlip)
            StartFlipping();
        ControlledBook.OnFlip.AddListener(new UnityEngine.Events.UnityAction(PageFlipped));
	}
    void PageFlipped()
    {
        isFlipping = false;
        ControlledBook.interactable = true;
    }
	public void StartFlipping()
    {
        StartCoroutine(FlipToEnd());
    }
    public void FlipRightPage()
    {
        if (isFlipping) return;
        if (isWiggling)
        {
            StopWiggling(() => FlipRightPage());
            return;
        }

        if (ControlledBook.currentPage >= ControlledBook.TotalPageCount) return;
        if (ControlledBook.OnReleaseEvent != null) ControlledBook.OnReleaseEvent.Invoke();
        ControlledBook.interactable = false;
        isFlipping = true;
        float xc = (ControlledBook.EndBottomRight.x + ControlledBook.EndBottomLeft.x) / 2;
        float xl = ((ControlledBook.EndBottomRight.x - ControlledBook.EndBottomLeft.x) / 2) * 0.9f;
        //float h =  ControledBook.Height * 0.5f;
        float h = Mathf.Abs(ControlledBook.EndBottomRight.y) * 0.9f;
        StartCoroutine(FlipRTL(xc, xl, h));
    }
    public void FlipLeftPage()
    {
        if (isFlipping) return;
        if (ControlledBook.currentPage <= 0) return;
        if (ControlledBook.OnReleaseEvent != null) ControlledBook.OnReleaseEvent.Invoke();
        ControlledBook.interactable = false;
        isFlipping = true;
        float xc = (ControlledBook.EndBottomRight.x + ControlledBook.EndBottomLeft.x) / 2;
        float xl = ((ControlledBook.EndBottomRight.x - ControlledBook.EndBottomLeft.x) / 2) * 0.9f;
        //float h =  ControledBook.Height * 0.5f;
        float h = Mathf.Abs(ControlledBook.EndBottomRight.y) * 0.9f;
        StartCoroutine(FlipLTR(xc, xl, h));
    }
    IEnumerator FlipToEnd()
    {
        yield return new WaitForSeconds(DelayBeforeStarting);
        float xc = (ControlledBook.EndBottomRight.x + ControlledBook.EndBottomLeft.x) / 2;
        float xl = ((ControlledBook.EndBottomRight.x - ControlledBook.EndBottomLeft.x) / 2)*0.9f;
        //float h =  ControledBook.Height * 0.5f;
        float h = Mathf.Abs(ControlledBook.EndBottomRight.y)*0.9f;
        //y=-(h/(xl)^2)*(x-xc)^2          
        //               y         
        //               |          
        //               |          
        //               |          
        //_______________|_________________x         
        //              o|o             |
        //           o   |   o          |
        //         o     |     o        | h
        //        o      |      o       |
        //       o------xc-------o      -
        //               |<--xl-->
        //               |
        //               |
        switch (Mode)
        {
            case FlipMode.RightToLeft:
                while (ControlledBook.currentPage < ControlledBook.TotalPageCount)
                {
                    StartCoroutine(FlipRTL(xc, xl, h));
                    yield return new WaitForSeconds(TimeBetweenPages);
                }
                break;
            case FlipMode.LeftToRight:
                while (ControlledBook.currentPage > 0)
                {
                    StartCoroutine(FlipLTR(xc, xl, h));
                    yield return new WaitForSeconds(TimeBetweenPages);
                }
                break;
        }
    }
    public void StartWiggling()
    {
        if (ControlledBook.currentPage >= ControlledBook.TotalPageCount) return;
        ControlledBook.interactable = false;
        isWiggling = true;

        float xc = (ControlledBook.EndBottomRight.x + ControlledBook.EndBottomLeft.x) / 2;
        float xl = ((ControlledBook.EndBottomRight.x - ControlledBook.EndBottomLeft.x) / 2) * .96f;
        float h = Mathf.Abs(ControlledBook.EndBottomRight.y) * .4f;

        StartCoroutine(WiggleCorner(xc, xl, h));
    }
    public void StopWiggling(Action handler = null)
    {
        if (isWiggling)
        {
            afterWiggleHandler = handler;
            isWiggling = false;
        }
    }
    IEnumerator FlipRTL(float xc, float xl, float h)
    {
        float x = xc + xl;
        float startingX = x;
        float y = (-h / (xl * xl)) * (x - xc) * (x - xc);

        ControlledBook.DragRightPageToPoint(new Vector3(x, y, 0));
        while (x > -startingX)
        {
            y = (-h / (xl * xl)) * (x - xc) * (x - xc);
            ControlledBook.UpdateBookRTLToPoint(new Vector3(x, y, 0));
            x -= 100f * flipSpeed * Time.deltaTime;
            yield return null;
        }
        ControlledBook.ReleasePage(false);
    }
    IEnumerator FlipLTR(float xc, float xl, float h)
    {
        float x = xc - xl;
        float startingX = x;
        float y = (-h / (xl * xl)) * (x - xc) * (x - xc);

        ControlledBook.DragLeftPageToPoint(new Vector3(x, y, 0));
        while (x < -startingX)
        {
            y = (-h / (xl * xl)) * (x - xc) * (x - xc);
            ControlledBook.UpdateBookLTRToPoint(new Vector3(x, y, 0));
            x += 100f * flipSpeed * Time.deltaTime;
            yield return null;
        }
        ControlledBook.ReleasePage(false);
    }

    IEnumerator WiggleCorner(float xc, float xl, float h)
    {
        float x = xc + xl;

        Func<float, float> getNewY = (x) => (-h / (xl * xl)) * (x - xc) * (x - xc) - .56f * Mathf.Abs(ControlledBook.EndBottomRight.y);
        float y = getNewY(x);

        bool wiggleToLeft = true;
        float maxWiggle = x * (1f - wigglePercent);
        float minWiggle = x;
        float t = (float)Math.PI / 2;

        ControlledBook.DragRightPageToPoint(new Vector3(x, y, 0));
        while (isWiggling)
        {
            y = getNewY(x);
            ControlledBook.UpdateBookRTLToPoint(new Vector3(x, y, 0));
            yield return null;

            if (wiggleToLeft && x < maxWiggle) wiggleToLeft = false;
            else if (!wiggleToLeft && x > minWiggle) wiggleToLeft = true;

            t += Time.deltaTime * wiggleSpeed;
            x = (float)Math.Sin(t) * (minWiggle - maxWiggle) / 2 + (minWiggle + maxWiggle) / 2;
        }

        ControlledBook.ReleaseWiggle(afterWiggleHandler);
        ControlledBook.interactable = true;
    }
}
