using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Base;
using BH.oM.Structural.Elements;
using BH.oM.Structural.Properties;
using BH.oM.Structural.Loads;
using BH.oM.Common.Materials;
using RAMDATAACCESSLib;
using System.IO;
using BH.oM.Geometry;


namespace BH.Adapter.RAM
{
    public partial class RAMAdapter
    {
        /***************************************************/
        /**** Adapter overload method                   ****/
        /***************************************************/
        protected override IEnumerable<IBHoMObject> Read(Type type, IList ids)
        {
            //Choose what to pull out depending on the type. Also see example methods below for pulling out bars and dependencies
            if (type == typeof(Node))
                return ReadNodes(ids as dynamic);
            else if (type == typeof(Bar))
                return ReadBars(ids as dynamic);
            else if (type == typeof(ISectionProperty) || type.GetInterfaces().Contains(typeof(ISectionProperty)))
                return ReadSectionProperties(ids as dynamic);
            else if (type == typeof(Material))
                return ReadMaterials(ids as dynamic);
            else if (type == typeof(PanelPlanar))
                return ReadPanels(ids as dynamic);
            if (type == typeof(Loadcase))
                return ReadLoadCase();


            return null;
        }

        /***************************************************/
        /**** Private specific read methods             ****/
        /***************************************************/

        //The List<string> in the methods below can be changed to a list of any type of identification more suitable for the toolkit

        private List<Bar> ReadBars(List<string> ids = null)
        {
            //Implement code for reading bars
            List<Bar> bhomBars = new List<Bar>();

            IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            // Get stories
            IStories IStories = IModel.GetStories();
            int numStories = IStories.GetCount();

            // Get all elements on each story
            for (int i = 0; i < numStories; i++)
            {
                //Get Columns
                IColumns IColumns = IStories.GetAt(i).GetColumns();
                int numColumns = IColumns.GetCount();

                //Get Beams
                ILayoutBeams ILayoutBeams = IStories.GetAt(i).GetFloorType().GetLayoutBeams();
                IBeams IBeams = IStories.GetAt(i).GetBeams();
                int numBeams = ILayoutBeams.GetCount();

                //Get Vertical Braces
                IVerticalBraces IVBraces = IStories.GetAt(i).GetVerticalBraces();
                int numVBraces = IVBraces.GetCount();

                //Get Horizontal Braces
                ILayoutHorizBraces ILayoutHorizBraces = IStories.GetAt(i).GetFloorType().GetLayoutHorizBraces();
                IHorizBraces IHorizBraces = IStories.GetAt(i).GetHorizBraces();
                int numHBraces = ILayoutHorizBraces.GetCount();

                //Get Elevation
                double dElevation = IStories.GetAt(i).dElevation;

                // Convert Columns
                for (int j = 0; j < numColumns; j++)
                {
                    IColumn IColumn = IColumns.GetAt(j);
                    Bar bhomBar = BH.Engine.RAM.Convert.ToBHoMObject(IColumn);
                    bhomBars.Add(bhomBar);
                }

                // Convert Beams
                for (int j = 0; j < numBeams; j++)
                {
                    IBeam IBeam = IBeams.GetAt(j);
                    ILayoutBeam ILayoutBeam = ILayoutBeams.GetAt(j);
                    Bar bhomBar = BH.Engine.RAM.Convert.ToBHoMObject(IBeam, ILayoutBeam, dElevation);
                    bhomBars.Add(bhomBar);
                }

                // Convert Vertical Braces
                for (int j = 0; j < numVBraces; j++)
                {
                    IVerticalBrace IVerticalBrace = IVBraces.GetAt(j);
                    Bar bhomBar = BH.Engine.RAM.Convert.ToBHoMObject(IVerticalBrace);
                    bhomBars.Add(bhomBar);
                }

                // Convert Horizontal Braces
                for (int j = 0; j < numHBraces; j++)
                {
                    IHorizBrace IHorizBrace = IHorizBraces.GetAt(j);
                    ILayoutHorizBrace ILayoutHorizBrace = ILayoutHorizBraces.GetAt(j);
                    Bar bhomBar = BH.Engine.RAM.Convert.ToBHoMObject(IHorizBrace, ILayoutHorizBrace, dElevation);
                    bhomBars.Add(bhomBar);
                }

            }

            return bhomBars;


        }

        /***************************************/

        private List<Node> ReadNodes(List<string> ids = null)
        {
            //Implement code for reading nodes
            List<Node> bhomNodes = new List<Node>();

            IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            INodes INodes = IModel.GetFrameAnalysisNodes();
            int numNodes = INodes.GetCount();

            for (int i = 0; i < numNodes; i++)
            {
                //Get Nodes
                INode INode = INodes.GetAt(i);
                Node bhomNode = BH.Engine.RAM.Convert.ToBHoMObject(INode);
                bhomNodes.Add(bhomNode);
            }

                return bhomNodes;
        }

        /***************************************/

        private List<ISectionProperty> ReadSectionProperties(List<string> ids = null)
        {



            throw new NotImplementedException();
        }

        /***************************************/

        private List<Material> ReadMaterials(List<string> ids = null)
        {
            //Implement code for reading materials
            throw new NotImplementedException();

        }

        private List<Loadcase> ReadLoadCase(List<string> ids = null)
        {
            //Implement code for reading loadcases
            List<Loadcase> bhomLoadCases = new List<Loadcase>();

            IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);
            ILoadCases ILoadCases = IModel.GetLoadCases(EAnalysisResultType.DefaultResultType);

            for (int i = 0; i < ILoadCases.GetCount(); i++)
            {
                //Get Loadcases
                ILoadCase LoadCase = ILoadCases.GetAt(i);
                Loadcase bhomLoadcase = BH.Engine.RAM.Convert.ToBHoMObject(LoadCase);
                bhomLoadCases.Add(bhomLoadcase);
            }


            return bhomLoadCases;

        }

        /***************************************************/

        // Read panels method; will need to figure out how to convert geometry (RAM provides four corner points); does not seem to be working properly, crashes Rhino when walls are present
        private List<PanelPlanar> ReadPanels(List<string> ids = null)
        {
            //Implement code for reading panels
            List<PanelPlanar> bhomPanels = new List<PanelPlanar>();

            IModel IModel = m_RAMApplication.GetDispInterfacePointerByEnum(EINTERFACES.IModel_INT);

            //Code for accessing IWallPanels, which need to go through the IWallDesignGroups interface that is accessed directly through IModel
            IWallDesignGroups IWallDesignGroups = IModel.GetWallDesignGroups();

            for (int i = 0; i < IWallDesignGroups.GetCount(); i++)
            {
                IWallDesignGroup IWallDesignGroup = IWallDesignGroups.GetAt(i);
                IWallPanels IWallPanels = IWallDesignGroup.GetWallPanels();

                for (int j = 0; j < IWallPanels.GetCount(); j++)
                {
                    IWallPanel IWallPanel = IWallPanels.GetAt(j);

                    //CAUTION: Will assume all walls are the same height, cannot handle variable-height walls as written
                    PanelPlanar Panel = BH.Engine.RAM.Convert.ToBHoMObject(IWallPanel);
                    bhomPanels.Add(Panel);
                }
            }

            // Get stories
            IStories IStories = IModel.GetStories();
            int numStories = IStories.GetCount();

            // Get all elements on each story
            for (int i = 0; i < numStories; i++)
            {

                ////Get Walls (from IWALL DIRECTLY--SINCE IWALL CAN BE NON-PLANAR, THIS DOES NOT RETURN CORRECT RESULTS)
                //IWalls IWalls = IStories.GetAt(i).GetWalls();
                //int numWalls = IWalls.GetCount();

                //// Convert Walls
                //for (int j = 0; j < numWalls; j++)
                //{
                //    IWall IWall = IWalls.GetAt(j);
                //    PanelPlanar Panel = BH.Engine.RAM.Convert.ToBHoMObject(IWall);
                //    bhomPanels.Add(Panel);
                //}

                //Get Floors
                IStory IStory = IStories.GetAt(i);
                IFloorType IFloorType = IStory.GetFloorType();
                IDecks IDecks = IFloorType.GetDecks();
                int IStoryUID = IStory.lUID;

                int numDecks = IDecks.GetCount();


                // Convert Floors
                for (int j = 0; j < numDecks; j++)
                {
                    IDeck IDeck = IDecks.GetAt(j);
                    PanelPlanar Panel = BH.Engine.RAM.Convert.ToBHoMObject(IDeck, IStoryUID);
                    bhomPanels.Add(Panel);
                }

            }

            return bhomPanels;
        }
    }
}
