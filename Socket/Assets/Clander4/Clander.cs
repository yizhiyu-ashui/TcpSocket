using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 脚本挂在ScrollView上
/// </summary>
public class Clander : MonoBehaviour ,IDragHandler{
    //滚动视图
    public ScrollRect year;
    //鼠标点击和释放时候的坐标  通过eventData获取
    Vector3 pressPos;
    Vector3 releasePos;

    //拖拽的距离
    float distance;
    //滚动视图content下物体的高
    float itemHeight;
    //年份
    int middleYear = 1949;
    //选择的年份
    string selectYeay;
    //拖拽触发
    public void OnDrag(PointerEventData eventData)
    {
        //  ContentItemMove(eventData.delta);
        // print(eventData.delta);  delta是一个动态的值
        pressPos =  eventData.pressPosition;
        releasePos = eventData.position;
        //求距离
        distance = Mathf.Abs(releasePos.y - pressPos.y);
        
       ContentItemMove(eventData.delta);

        YearChange();
    }

    // Use this for initialization
    void Start () {

        itemHeight = year.content.GetChild(0).GetComponent<RectTransform>().rect.height;

        year.content.GetChild(2).GetComponent<Text>().text = middleYear.ToString();
    }
	
	// Update is called once per frame
	void Update () {
        
    }
    /// <summary>
    /// 用来移动视图
    /// </summary>
    /// <param name="move"></param>
    void ContentItemMove(Vector2 move)
    {
        //当移动距离大于一个item的时候
        if (distance > itemHeight) 
        {
            for (int i = 0; i < year.content.childCount; i++)
            {
                //上移
                if (releasePos.y >pressPos.y)
                {
                    //
                    year.content.GetChild(i).GetComponent<RectTransform>().anchoredPosition += new Vector2(0 ,move.y );
                }
                else //下移
                {
                    year.content.GetChild(i).GetComponent<RectTransform>().anchoredPosition += new Vector2(0,move.y);
                }
            }
        }
    }
    //无限滚动和改变年份（有Bug）,思路就是在滚动视图中有四个item   显示界面中最中间的item是基准，  也就是content中索引是2的子物体   这个索引是2的物体是一直在改变的
    void YearChange()
    {
        for (int i = 0;i<year.content.childCount;i++)
        {
            //处于索引2的时候，字体是白色  其余的是黑色  用来突出选择的年份
            if (year.content.GetChild(i).GetComponent<RectTransform>().anchoredPosition.y > -15 && year.content.GetChild(i).GetComponent<RectTransform>().anchoredPosition.y < 15)
            {
                year.content.GetChild(i).GetComponent<Text>().color = Color.white;
                //把选择的年份赋值到变量中，一遍获取
                selectYeay = year.content.GetChild(i).GetComponent<Text>().text;
            }
            else
            {
                year.content.GetChild(i).GetComponent<Text>().color = Color.black;
            }
            
        }
        //在拖拽的过程中，作为基准的索引2的相对位置的y值大于15的时候（整个item的高是30）   把最上面的item移动到最下面
        if (year.content.GetChild(2).GetComponent<RectTransform>().anchoredPosition.y > 15)
        {
            //更改默认年份
            middleYear = middleYear + 1;
            //改所i有item的索引
            year.content.GetChild(0).SetAsLastSibling();
            //移动 放到最后一个item的下面
            year.content.GetChild(0).GetComponent<RectTransform>().anchoredPosition = year.content.GetChild(3).GetComponent<RectTransform>().anchoredPosition - new Vector2(0, itemHeight);
            //更改显示的年份
            year.content.GetChild(0).GetComponent<Text>().text = (middleYear + 2).ToString();
        }
        else if (year.content.GetChild(2).GetComponent<RectTransform>().anchoredPosition.y < -45)
        {
            middleYear = middleYear - 1;
            year.content.GetChild(3).SetAsFirstSibling();
            year.content.GetChild(3).GetComponent<RectTransform>().anchoredPosition = year.content.GetChild(0).GetComponent<RectTransform>().anchoredPosition + new Vector2(0, itemHeight);
            year.content.GetChild(3).GetComponent<Text>().text = (middleYear - 1).ToString();
        }
    }
    public void ConfirmYear()
    {
        print("当前选择年份" + selectYeay);
    }
}
