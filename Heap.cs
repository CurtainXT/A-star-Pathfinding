using System;
using System.Collections;
using UnityEngine;

public class Heap<T> where T : IHeapItem<T>
{
    T[] items;
    int currentItemCount;

    public Heap(int maxHeapSize) //考虑到我们用的数组不好调整大小 所以指定堆的大小
    {
        items = new T[maxHeapSize];
    }

    // 添加元素
    public void Add(T item)
    {
        item.HeapIndex = currentItemCount;
        items[currentItemCount] = item;
        SortUp(item);
        currentItemCount++;
    }

    // 移除并获取堆的第一个元素 第一个元素优先级总是最小的
    public T RemoveFirst()
    {
        T firstItem = items[0];
        currentItemCount--;
        items[0] = items[currentItemCount]; //取堆末尾的元素填到最前
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return firstItem;
    }

    // 更新元素在堆中的位置
    public void UpdateItem(T item)
    {
        // 这两个最多有一个有用 不存在冲突
        SortUp(item);
        SortDown(item);
    }

    public int Count
    {
        get { return currentItemCount; }
    }

    // 堆中是否包含item
    public bool Contains(T item)
    {
        return Equals(items[item.HeapIndex], item);
    }

    // 将父节点与其子节点做比较并交换
    void SortDown(T item)
    {
        while(true)
        {
            int childIndexLeft = item.HeapIndex * 2 + 1;
            int childIndexRight = item.HeapIndex * 2 + 2;
            int swapIndex = 0;

            if(childIndexLeft < currentItemCount) //确保我们没有超出范围
            {
                // 检查两个子节点的优先级最高的是谁
                swapIndex = childIndexLeft;
                if(childIndexRight < currentItemCount)
                {
                    if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                    {
                        swapIndex = childIndexRight;
                    }    
                }
                // 检查父节点与最高优先级子节点的优先级
                if (items[swapIndex].CompareTo(item) > 0)
                {
                    Swap(items[swapIndex], item); //子节点需要更靠前
                }
                else
                {
                    return; //item在当前位置符合堆的要求
                }
            }
            else
            {
                return; // 没有子节点
            }

        }
    }

    // 将子节点与其父节点做比较并交换
    void SortUp(T item)
    {
        int parentIndex = (item.HeapIndex - 1) / 2;
        while(true)
        {
            T parentItem = items[parentIndex];
            if(item.CompareTo(parentItem) > 0)
            {
                Swap(item, parentItem); //子节点需要更靠前
            }
            else
            {
                break; //item在当前位置符合堆的要求
            }
            // 计算新的parent
            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    void Swap(T itemA, T itemB)
    {
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;

        int itemAIndex = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex; 
        itemB.HeapIndex = itemAIndex;
    }
}

public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex
    {
        get;
        set;
    }
}