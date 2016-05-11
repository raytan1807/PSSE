﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static Pokemon_Shuffle_Save_Editor.ToolFunctions;

namespace Pokemon_Shuffle_Save_Editor
{
    public partial class Cheats : Form
    {
        public Cheats(Database db)
        {
            InitializeComponent();
            mondata = db.MonData;
            monlevel = db.MonLevel;
            stagesMain = db.StagesMain;
            stagesEvent = db.StagesEvent;
            stagesExpert = db.StagesExpert;
            megaStone = db.MegaStone;
            HasMega = db.HasMega;
            savedata = db.SaveData;
            mons = db.Mons;
            megas = db.Megas;            
            megalist = db.MegaList;
            megaArray_start = db.MegaStartIndex;
            dtb = db; 
        }

        private byte[] savedata;
        private byte[] mondata;
        private byte[] monlevel;
        private byte[] stagesMain;
        private byte[] stagesEvent;
        private byte[] stagesExpert;
        private byte[] megaStone;
        private Tuple<int, int, bool, int, int, int, int, Tuple<int>>[] mons; //specieIndex, formIndex, isMega, raiseMaxLevel, basePower, talent, type, rest
        private Tuple<int, int>[] megas; //monsIndex, speedups
        private List<int> megalist;
        private int megaArray_start;
        private bool[][] HasMega; // [X][0] = X, [X][1] = Y
        Database dtb;

        private void B_CaughtEverything_Click(object sender, EventArgs e)
        {
            for (int i = 1; i < megaArray_start; i++) //includes 15 reserved slots
                SetPokemon(dtb, i, true);
            MessageBox.Show("All Pokemon are now caught.");
        }

        private void B_CaughtObtainables_Click(object sender, EventArgs e)
        {
            for (int i = 1; i < megaArray_start; i++) //includes 15 reserved slots
            {
                if ((mons[i].Rest.Item1 != 999) && ((mons[i].Item5 != 1) || (mons[i].Item6 != 1) || (mons[i].Item7 != 0)))
                    SetPokemon(dtb, i, true);
                else SetPokemon(dtb, i, false);
            }
            int stagelen = BitConverter.ToInt32(stagesMain, 0x4);
            int Num_MainStages = BitConverter.ToInt32(stagesMain, 0);
            int Num_ExpertStages = BitConverter.ToInt32(stagesExpert, 0);
            for (int i = 1; i < Num_MainStages; i++)
            {
                int ind = BitConverter.ToUInt16(stagesMain, 0x50 + stagelen * (i)) & 0x3FF;
                SetPokemon(dtb, ind, true);
            }
            for (int i = 1; i < Num_ExpertStages; i++)
            {
                int ind = BitConverter.ToUInt16(stagesExpert, 0x50 + stagelen * (i)) & 0x3FF;
                SetPokemon(dtb, ind, true);
            }
            MessageBox.Show("All obtainable Pokemon are now caught.");
        }

        private void B_AllStones_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < megaArray_start; i++)
            {
                if (HasMega[mons[i].Item1][0] || HasMega[mons[i].Item1][1])
                    SetMegaStone(dtb, i, HasMega[mons[i].Item1][0], HasMega[mons[i].Item1][1]);
            }
            MessageBox.Show("All Mega Stones are now owned.");
        }

        private void B_AllCaughtStones_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < megaArray_start; i++)
            {
                if (GetPokemon(dtb, i))
                {
                    if (HasMega[mons[i].Item1][0] || HasMega[mons[i].Item1][1])
                    {
                        if ((mons[megaArray_start + megalist.IndexOf(i)].Item6 != 7) || (mons[megaArray_start + megalist.IndexOf(i)].Item7 != 0) || (megas[megalist.IndexOf(i)].Item2 != 1)) //Checks type, "talent" & max speedups.Doesn't check if Y form has been released, but both Charizard's & Mewtwo's already have.
                            SetMegaStone(dtb, i, HasMega[mons[i].Item1][0], HasMega[mons[i].Item1][1]);
                    }
                }
            }
            MessageBox.Show("All available megastones have been owned for everything you've caught.");
        }

        private void B_LevelMax_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < megaArray_start; i++) //Includes 15 reserved slots
            {
                if (GetPokemon(dtb, i))
                {
                    //Reads the max amount of lollipops for that pokemon & set level to Max.
                    int numRaiseMaxLevel = Math.Min(mons[i].Item4, 5);
                    int max = 10 + numRaiseMaxLevel;
                    SetLevel(dtb, i, max);
                }
            }
            MessageBox.Show("Everything you've caught is now level Max.");
        }

        private void B_MaxResources_Click(object sender, EventArgs e)
        {
            int[] items = new int[7];
            for (int i = 0; i < 7; i++)
                items[i] = 99;
            int[] enhancements = new int[9];
            for (int i = 0; i < 9; i++)
                enhancements[i] = 99;
            SetResources(dtb, 99, 99999, 150, items, enhancements);
            MessageBox.Show("Gave 99 hearts, 99999 coins, 150 jewels, and 99 of every item.");
        }

        private void B_MaxSpeedups_Click(object sender, EventArgs e)
        {            
            for (int i = 0; i < megaArray_start; i++)
            {
                if (GetPokemon(dtb, i))
                {
                    if (HasMega[mons[i].Item1][0] || HasMega[mons[i].Item1][1])
                        SetMegaSpeedup(dtb, i, HasMega[mons[i].Item1][0], HasMega[mons[i].Item1][1]);
                }
            }
            MessageBox.Show("All Owned Megas have been fed with as much Mega Speedups as possible.");
        }

        private void B_AllCompleted_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < (BitConverter.ToInt32(stagesMain, 0) - 1); i++)
                SetStage(dtb, i, 0, true);
            for (int i = 0; i < BitConverter.ToInt32(stagesExpert, 0); i++)
                SetStage(dtb, i, 1, true);
            MessageBox.Show("All Normal & Expert stages have been marked as completed.\n\nRewards like megastones or jewels can still be redeemed by beating the stage.");
        }

        private void B_SRankCompleted_Click(object sender, EventArgs e) 
        {  
            int entrylen = BitConverter.ToInt32(stagesMain, 0x4);
            int rank = 3;
            for (int i = 0; i < (BitConverter.ToInt32(stagesMain, 0) - 1); i++)
            {
                if (GetStage(dtb, i, 0))
                    SetRank(dtb, i, 0, rank);
            }
            for (int i = 0; i < BitConverter.ToInt32(stagesExpert, 0); i++)
            {
                if (GetStage(dtb, i, 1))
                    SetRank(dtb, i, 1, rank);
            }
            MessageBox.Show("All Completed Normal & Expert stages have been S-ranked.");
        }

        private void B_StreetPassDelete_Click(object sender, EventArgs e)
        {
            Array.Copy(BitConverter.GetBytes(0x0000), 0, savedata, 0x5967, 2); //Resets streetpass count to 0
            byte[] blank = new byte[0x68];
            for (int i = 0; i < 10; i++)
                Array.Copy(blank, 0, savedata, 0x59A7 + (i * 0x68), 0x68); //Erase StreetPass tags
            MessageBox.Show("StreetPass data have been cleared & StreetPass count reset to 0.");
        }

        private void B_MaxExcalationBattle_Click(object sender, EventArgs e)
        {
            SetExcalationStep(dtb, 999);
            MessageBox.Show("Curent escalation battle has been taken to step 999. You'll get all rewards at once by beating it.\n\nCarefull : only use it when there's exactly one active escalation battle.\nI don't know how this behaves if there is 0 or more than 1 active at the same time.");
        }
        
        private void B_PokemonReset_Click(object sender, EventArgs e)
        {
            for (int i = 1; i < megaArray_start; i++) //Uncatch
                ToolFunctions.SetPokemon(dtb, i, false);
            for (int i = 0; i < megaArray_start; i++) //Un-level, Un-experience & Un-lollipop
                SetLevel(dtb, i);    
            for (int i = 0; i < megaArray_start; i++) //Un-stone
            {
                if (HasMega[mons[i].Item1][0] || HasMega[mons[i].Item1][1])
                    SetMegaStone(dtb, i, false, false);
            }
            for (int i = 0; i < megaArray_start; i++) //Unfeed speedups
            {
                if (HasMega[mons[i].Item1][0] || HasMega[mons[i].Item1][1])
                    SetMegaSpeedup(dtb, i, false, false);
            }
            MessageBox.Show("All pokemons have been uncaught, reset to level 1 & lost their Mega Stones, speedups or lollipops.\n\nEither reset stages too or make sure to catch at least Espurr, Bulbasaur, Squirtle & Charmander manually.");
        }

        private void B_StageReset_Click(object sender, EventArgs e)
        {
            int[] lengths = { (BitConverter.ToInt32(stagesMain, 0) - 1), BitConverter.ToInt32(stagesExpert, 0), 100 };  //max number of event levels should be 549 but 100 should be enough to wipe all data from event stages at any time
            int j = 0;
            foreach (int length in lengths)
            {                
                for (int i = 0; i < length; i++)
                {
                    SetStage(dtb, i, j);
                    SetRank(dtb, i, j);
                    SetScore(dtb, i, j);
                }
                j++;
            }
            MessageBox.Show("All stages have been reset to C Rank, 0 score & uncompleted state.");
        }

        private void B_ResourcesReset_Click(object sender, EventArgs e)
        {            
            SetResources(dtb);
            MessageBox.Show("Deleted all stock hearts, coins, jewels and Items.");
        }

        private void B_EscalationReset_Click(object sender, EventArgs e)
        {
            SetExcalationStep(dtb);
            MessageBox.Show("Curent escalation battle has been reverted to step 1.\n\nCarefull : only use it when there's exactly one active escalation battle.\nI don't know how this behaves if there is 0 or more than 1 active at the same time.");
        }
        
        protected override bool ProcessDialogKey(Keys keyData)  //Allows quit when Esc is pressed
        {
            if (Form.ModifierKeys == Keys.None && keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

    }
}
