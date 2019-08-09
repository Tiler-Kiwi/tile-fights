using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Credit to catlike coding



[System.Serializable]
public struct HexCoordinates : IEquatable<HexCoordinates>
{

    [SerializeField]
    private int _x, _z, _depth;

    public int X { get { return _x; } }
    public int Z { get { return _z; } }
    public int Y { get { return -X - Z; } set { } }

    /*
    The sum of XYZ always equals 0. 
    You therefore only need to store 2 values.
    Y is derived from -X - Z
    So if X = 1, Y=2, Z= -3 (1+2+-3 = 0)
    */

    public int Depth { get { return _depth; } }

    public HexCoordinates(int x, int z, int height)
    {
        this._x = x;
        this._z = z;
        this._depth = height;
    }

    public static HexCoordinates FromOffsetCoordinates(int col, int row, int height)
    {
        int x = col - (row - (row & 1)) / 2;
        int z = row;
        return new HexCoordinates(x,z,height); // z=z, x = x-(z/2). z always rounds down.
    }

    public static HexCoordinates FromOffsetCoordinates(Vector3 offsets)
    {
        return FromOffsetCoordinates((int)offsets.x, (int)offsets.z, (int)offsets.y);
    }

    /// <summary>
    /// Returns X, Y, Z coords of tile.
    /// </summary>
    /// <param name="cube"></param>
    /// <returns>XYZ, X=Column, Y=Depth, Z=Row</returns>
    public static Vector3 ToOffsetCoordinates(HexCoordinates cube)
    {
        float col = cube.X + (cube.Z - (cube.Z & 1)) / 2.0f;
        float row = cube.Z;
        return new Vector3(col, cube.Depth, row);
    }

    internal static HexCoordinates? TransformsToCoords(Vector3 TransformCoords)
    {
        float offset = HexMetrics.innerRadius * ((int)TransformCoords.z & 1);
        Vector3 offsets = new Vector3(TransformCoords.x / (HexMetrics.innerRadius * 2f + offset), TransformCoords.y / HexMetrics.elevationStep, -1 * TransformCoords.z / (HexMetrics.outerRadius * 1.5f));

        int x = (int)(offsets.x - (offsets.z - ((int)offsets.z & 1)) / 2);
        int z = (int)offsets.z;
        int depth = (int)offsets.y;
        return new HexCoordinates(x, z, depth);
    }

    public override string ToString()
    {
        return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ", " + Depth.ToString() + ")"; //used in some debuggin stuff
    }

    public string ToStringOnSeperateLines()
    {
        return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString(); //used in the console app i guess, prob could stand to return an array or something
    }

    private static HexCoordinates FromPosition(Vector3 position)
    {
        //From Catlike Coding, not sure if compatible at all anymore
        float x = position.x / (HexMetrics.innerRadius * 2f);
        float y = -x;
        float offset = position.z / (HexMetrics.outerRadius * 3f);
        x -= offset;
        y -= offset;

        float height = position.y / (HexMetrics.elevationStep);

        int iX = Mathf.RoundToInt(x);
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(-x - y);
        int iH = Mathf.RoundToInt(height);

        if (iX + iY + iZ != 0)
        {
            float dX = Mathf.Abs(x - iX);
            float dY = Mathf.Abs(y - iY);
            float dZ = Mathf.Abs(-x - y - iZ);

            if (dX > dY && dX > dZ)
            {
                iX = -iY - iZ;
            }
            else if (dZ > dY)
            {
                iZ = -iX - iY;
            }
        }
        return new HexCoordinates(iX, iZ, iH);
    }

    //Static methods

    public static float Distance(HexCoordinates start, HexCoordinates end)
    {
        float HexDist = (Mathf.Abs(start.X - end.X) + Mathf.Abs(start.Y - end.Y) + Mathf.Abs(start.Z - end.Z)) * 0.5f;
        float DepthDist = Mathf.Abs(start.Depth - end.Depth);
        return Mathf.Max(HexDist, DepthDist);
    }

    //Stuff from P_CubeCoords

    /*
	My own implementation of the Cube coordinate system detailed here https://www.redblobgames.com/grids/hexagons/#coordinates
*/

        public HexCoordinates UpRight
        {
            get
            {
                return new HexCoordinates(X+1, Z - 1, Depth);
            }
            set { }
        }

        public HexCoordinates UpLeft
        {
            get
            {
                return new HexCoordinates(X, Z - 1, Depth);
            }
            set { }
        }

        public HexCoordinates Right
        {
            get
            {
                return new HexCoordinates(X + 1, Z, Depth);
            }
            set { }
        }

        public HexCoordinates Left
        {
            get
            {
                return new HexCoordinates(X - 1, Z, Depth);
            }
            set { }
        }

        public HexCoordinates DownRight
        {
            get
            {
                return new HexCoordinates(X, Z + 1, Depth);
            }
            set { }
        }

        public HexCoordinates DownLeft
        {
            get
            {
                return new HexCoordinates(X-1, Z + 1, Depth);
            }
            set { }
        }

    public HexCoordinates Below { get { return new HexCoordinates(X, Z, Depth - 1); } set { } }
    public HexCoordinates Above { get { return new HexCoordinates(X, Z, Depth + 1); } set { } }

    /// <summary>
    /// Returns the directional vector which the target tile is closest towards. Ties are biased clockwise.
    /// </summary>
    /// <param name="targetTile"></param>
    /// <returns></returns>
    public HexDirection GetFacing (HexCoordinates targetTile)
    {
        //error on certain edge cases.
        //for now, biased for clockwise
        int dirX = targetTile.X - this.X;
        int dirY = targetTile.Y - this.Y;
        int dirZ = targetTile.Z - this.Z;

        if (dirX > dirY) // Eastwards
        {
            if(dirY > dirZ)
            {
                return HexDirection.NE;
            }
            if(dirX > dirZ)
            {
                return HexDirection.E;
            }
            if(dirZ > dirX)
            {
                return HexDirection.SE;
            }
        }
        else if(dirY > dirX) //Westwards
        {
            if (dirZ > dirY)
            {
                return HexDirection.SW;
            }
            if (dirZ > dirX)
            {
                return HexDirection.W;
            }
            if (dirX > dirZ)
            {
                return HexDirection.NW;
            }
        }

        //edge cases
        if (dirX == dirY) { if (dirY > dirZ) { return HexDirection.NE; } return HexDirection.SW; }
        if(dirX == dirZ) { if (dirX > dirY) { return HexDirection.SE; } return HexDirection.NW; }
        if (dirZ > dirX) { return HexDirection.W; }return HexDirection.E;
    }

    public HexCoordinates DirectionTransform(HexDirection dir, int depthChange)
    {
        HexCoordinates point = new HexCoordinates(this.X, this.Z, this.Depth + depthChange);
        switch (dir)
        {
            case HexDirection.SW:
                return point.DownLeft;
            case HexDirection.SE:
                return point.DownRight;
            case HexDirection.E:
                return point.Right;
            case HexDirection.NE:
                return point.UpRight;
            case HexDirection.NW:
                return point.UpLeft;
            case HexDirection.W:
                return point.Left;
        }
        throw new Exception("This should never appear!");
    }

    public HexCoordinates DirectionTransform(HexDirection3D dir)
    {
        if (dir == HexDirection3D.DOWN) { return new HexCoordinates(this.X, this.Z, this.Depth - 1); }
        if (dir == HexDirection3D.UP) { return new HexCoordinates(this.X, this.Z, this.Depth + 1); }
        return DirectionTransform((HexDirection)dir, 0);
    }
        public static HexCoordinates RoundedCubes(double X, double Y, double Z, double depth)
        {
            /*
			certain math shit will get you floating points for xyz values
			but you need rounded numbers for your stuff
			however, a simple rounding will result in errors
			such as nonsense values where x+y+z != 0.
			
			so this function serves to correct that.
			the number with the largest difference between its rounded value and its initial value is changed
			to val1 = -val2-val3
			*/
            double roundedX = System.Math.Round(X);
            double roundedY = System.Math.Round(Y);
            double roundedZ = System.Math.Round(Z);

            //diff is difference between initial value and rounded value. what is lost/gained when rounding.
            double x_diff = System.Math.Abs(roundedX - X);
            double y_diff = System.Math.Abs(roundedY - Y);
            double z_diff = System.Math.Abs(roundedZ - Z);

            if (x_diff > y_diff && x_diff > z_diff) //xdiff is biggest
            {
                roundedX = -1 * roundedY - roundedZ; //x is changed to accomidate the xyz=0 rule.
            }
            else if (y_diff > z_diff) //ydiff is biggest.
            {
                roundedY = -1 * roundedX - roundedZ; //y is changed instead.
            }
            else
            {
                roundedZ = -1 * roundedX - roundedY; //same story except with z
            }

            int intX = System.Convert.ToInt32(roundedX); //conversions to interger
           // int intY = System.Convert.ToInt32(roundedY);
            int intZ = System.Convert.ToInt32(roundedZ);

            return new HexCoordinates(intX, intZ, System.Convert.ToInt32(System.Math.Round(depth)));
        }
    internal List<HexCoordinates> GetCoordsWithinDistance(int range)
    {
        return GetCoordsWithinDistance(range, false, false);
    }

    /// <summary>
    /// Gets coords that are "range" spaces away. Includes source. Does not check if coords are valid or not!
    /// </summary>
    /// <param name="range"></param>
    /// <param name="checkUp"></param>
    /// <param name="checkDown"></param>
    /// <returns></returns>
    internal List<HexCoordinates> GetCoordsWithinDistance(int range, bool checkUp, bool checkDown)
    {
        List<HexCoordinates> ret = new List<HexCoordinates>();
        if (range == 0) { ret.Add(this); return ret; }
        if (checkUp)
        {
            int count = range;
            HexCoordinates check = this;
            while (count > 0)
            {
                check = check.Above; //look at coords above my own
                List<HexCoordinates> UpTiles = check.GetCoordsWithinDistance(range, false, false); //dont look up or down, just around in your range
                for (int i = 0; i < UpTiles.Count; i++)
                {
                    ret.Add(UpTiles[i]); //add tiles in range to return list
                }
                count--; //look as many spaces up as your range; returns a "box"
            }
        }
        if (checkDown)
        {
            int count = range; //same as above code but looking down now
            HexCoordinates check = this;
            while (count > 0)
            {
                check = check.Below;
                List<HexCoordinates> DownTiles = check.GetCoordsWithinDistance(range, false, false);
                for (int i = 0; i < DownTiles.Count; i++)
                {
                    ret.Add(DownTiles[i]);
                }
                count--;
            }
        }

        for (int x = -range; x<=range; x++)
        {
            for(int y = Math.Max(-range, -x-range); y<=Math.Min(range, -x+range);y++) //red blob games, i swear to god...
            {
                int z = -x - y;
                ret.Add(new HexCoordinates(this.X + x, this.Z + z, Depth)); //did i flip z and y...? does this still work...? (NO IT DIDNT IM STUPID AND FIXED IT)
            }
        }
        return ret;
    }

    public static double lerp(double a, double b, double t)
        {
            /*
			lerp is a fundamentaly important function present in a lot of shit
			from pathfinding to mapgeneration, or blurring colors. many things!
			
			really what it means is to create a new value "betweeen" existing values.
			
			its essentially the question of, given value a, and value b, find a point 't' between them
			the diff between a and b can be thought of as "distance", making t "time"
			i prefer to think of t as distance but theres some calculus shit behind the term so idk
			
			a + (b-a)*t,
			
			if t=0, its a
			if t=1, its b.
			
			otherwise, you get a value that is some "distance" between the two "points".
			
			t is usually a value between 0 and 1 but idk you can make t go further.
			imagining both a and b form a line of 1, a t value outside that could be a way to draw past that
			
			a=-1
			b= 1
			t= 2
			result is 3, which makes sense, as the difference between -1 and 1 is 2...
			so saying t=1 is like saying "give me value at dist X, with X being the different between a and b
			resulting in 1. 1 is 2 away from -1, 2 is the difference between a and b.
			saying "give me value at dist X 2" aka saying t=2, is like saying, "get new value from line of value change 
			defined by a and b"
			
			so its some value in the direction of 1, starting at -1, that is 2*abs(a-b) away, aka 4 "values" or whathaveyou
			-1 + 4 = 3.
			
			the problem is, this is just a 1D lerp. trying a t value outside 1 with a 2d or 3d or god help you a 4d lerp
			well it probably follows the same rules but what you're getting is probably some variety of madness. 
			god help you if value A and B exist in some kind of "curve" rather than a straight lerp like this
			since really those aren't even called "lerps" anymore, they're splines or something else insane 
			
			~CALCULUS~
			
			*/

            return a + (b - a) * t;
        }

        public static HexCoordinates cube_lerp(HexCoordinates a, HexCoordinates b, double t)
        {
            /*
			lerp each xyz value with another coords xyz, then round them properly.
			*/
            return RoundedCubes(lerp(a.X, b.X, t),
                                 lerp(a.Y, b.Y, t),
                                 lerp(a.Z, b.Z, t),
                                 lerp(a.Depth, b.Depth, t));
        }

    public bool NeighborOf(HexCoordinates target)
    {
        int x1 = this.X;
        int x2 = target.X;
        if (Math.Abs(x1 - x2) > 1) { return false; }
        int y1 = this.Y;
        int y2 = target.Y;
        if (Math.Abs(y1 - y2) > 1) { return false; }
        int z1 = this.Z;
        int z2 = target.Z;
        if (Math.Abs(z1 - z2) > 1) { return false; }
        int d1 = this.Depth;
        int d2 = target.Depth;
        if (Math.Abs(d1 - d2) > 1) { return false; }
        return true;
    }

    public static double cube_distance(HexCoordinates a, HexCoordinates b)
        {
        return Distance(a, b);
        }

        public static List<HexCoordinates> cube_linedraw(HexCoordinates a, HexCoordinates b)
        {
            //get all coords that exists in a line between point a and point b

            double N = cube_distance(a, b);

            List<HexCoordinates> results = new List<HexCoordinates>();

            if (N == 0)
            {
                return results;
            }

            for (int i = 0; i <= N; i++)
            {
                results.Add(cube_lerp(a, b, 1.0 / N * i));

                /*
				i goes up to N, so 1/N*i will get you a range of values between 0 and 1, for the t
				more specifically, it starts at 0, then every loop, grows by 1/N, until it reaches 1.
				
				if N == 4 it goes like, 0, 0.25, 0.5, 0.75, 1.
				*/
            }
            return results;
        }

        public static HexCoordinates cube_bez_curve(HexCoordinates a, HexCoordinates b, HexCoordinates c, double t)
        {
        /*
        A brez spline is a linear 3d lerp.
        */
        HexCoordinates thingab = cube_lerp(a, b, t); //first get a point between a and b
        HexCoordinates thingbc = cube_lerp(b, c, t); //then get a point between b and c
        HexCoordinates ReturnValue = cube_lerp(thingab, thingbc, t); //then get a point between the two lerped values

            /*
            the result is a curve with... specific properties
            i dont think i know it well enough to describe accuratly
            but essentially it will start at a, end at c, and wont touch b in most cases.
            */
            return ReturnValue;
        }

        public static List<HexCoordinates> cube_brezcurve_draw(HexCoordinates a, HexCoordinates b, HexCoordinates c)
        {
            //like linedraw, it gets points along a brezcurve defined by three points

            //double N = cube_distance(a, b) + cube_distance(b,c);
            double N = 5;
            List<HexCoordinates> results = new List<HexCoordinates>();
            if (N == 0)
            {
                return results;
            }

            for (int i = 0; i <= N; i++)
            {
                results.Add(cube_bez_curve(a, b, c, 1.0 / N * i));
            }
            return results;
        }

        public static List<HexCoordinates> cube_crude_brezspline_draw(List<HexCoordinates> nodes)
        {
            /*
            garbage
            no really, its just a means to get a wierd not-spline. 
            its not worth reading because what it produces is bad and should not be replicated

            otoh its how i was drawing rivers so it did its job just fine, mostly

            the idea is it links a bunch of points up via a wierd hack on the brez method
            by doing a cubic lerp on three points, then suddenly shifting at some t value to do
            the cubic lerp on some different values
            v1 v2 v3 -> v2 v3 v4 -> v3 v4 v5 and so on

            you get a curve, but not a spline. Its not a real slope, its not something that you could really describe
            with a particualr function, and you can't lerp up or down it via a t value.
            its just here to draw something like a spline.
            */
            List<HexCoordinates> CopiedNodes = new List<HexCoordinates>();

            for (int i = 0; i < nodes.Count; i++)
            {
                CopiedNodes.Add(nodes[i]);
            }
            if (CopiedNodes.Count < 3)
            {
                throw new System.Exception("I need three or more nodes to make a crude spline!");
            }
            List<HexCoordinates> results = new List<HexCoordinates>();

            for (int i = 0; i < CopiedNodes.Count - 3; i++)
            {
                //double N = cube_distance(CopiedNodes[i], CopiedNodes[i + 1]) + cube_distance(CopiedNodes[i+1], CopiedNodes[i + 2]);
                double N = 5; //i honestly dont know what the fuck this 5 is doing. my guess is that the mapgen always uses 5 nodes
                              //so i just cheated and set this to be 5 because i wasnt getting what i wanted otherwise.
                if (N == 0)
                {
                    continue;
                }
                for (int j = 0; j <= N / 2; j++)
                {
                    results.Add(cube_bez_curve(CopiedNodes[i],
                        CopiedNodes[i + 1],
                        CopiedNodes[i + 2],
                        1.0 / N * j));
                }
                CopiedNodes.RemoveAt(i + 1);
                CopiedNodes.Insert(i + 1, results[results.Count - 1]);
                //Debug.Log(CopiedNodes.Count + "THIS SHOULDNT BE SHRINKING");
            }

            List<HexCoordinates> LastCurve = HexCoordinates.cube_brezcurve_draw(CopiedNodes[CopiedNodes.Count - 3], CopiedNodes[CopiedNodes.Count - 2], CopiedNodes[CopiedNodes.Count - 1]);
            for (int i = 0; i < LastCurve.Count; i++)
            {
                results.Add(LastCurve[i]);
            }

            List<HexCoordinates> TrueResults = new List<HexCoordinates>();
            for (int i = 0; i < results.Count - 1; i++)
            {
                List<HexCoordinates> dumbshit = new List<HexCoordinates>();
                dumbshit = HexCoordinates.cube_linedraw(results[i], results[i + 1]);
                for (int j = 0; j < dumbshit.Count; j++)
                {
                    TrueResults.Add(dumbshit[j]);
                }
            }
            return TrueResults;
        }

        public static HexCoordinates StepForward(HexCoordinates point, HexDirection direction)
        {
            //this wasnt really used at anypoint iirc... i guess its fine, if sort of dumb.
            switch (direction)
            {
                case HexDirection.SW:
                    return point.DownLeft;
                case HexDirection.SE:
                    return point.DownRight;
                case HexDirection.E:
                    return point.Right;
                case HexDirection.NE:
                    return point.UpRight;
                case HexDirection.NW:
                    return point.UpLeft;
                case HexDirection.W:
                    return point.Left;
            }
            throw new System.Exception("What THE FUCK did you just say to me????");
        }

    public bool Equals(HexCoordinates other)
    {
        return (X == other.X && Y == other.Y && Z == other.Z && Depth == other.Depth);
    }

    public override bool Equals(object obj)
    {
        var other = obj as HexCoordinates?;
        if(other.HasValue)
        {
            return Equals(other.Value);
        }
        return false;
    }

    public static bool operator ==(HexCoordinates a, HexCoordinates b)
    {
        return a.Equals(b);
    }
    public static bool operator !=(HexCoordinates a, HexCoordinates b)
    {
        return (!a.Equals(b));
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static HexCoordinates operator +(HexCoordinates a, HexCoordinates b)
    {
        return new HexCoordinates(a.X + b.X, a.Z + b.Z, a.Depth + b.Depth);
    }
    public static HexCoordinates operator -(HexCoordinates a, HexCoordinates b)
    {
        return new HexCoordinates(a.X - b.X, a.Z - b.Z, a.Depth - b.Depth);
    }
}