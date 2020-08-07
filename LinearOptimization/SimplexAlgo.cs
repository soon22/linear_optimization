using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace LinearOptimization
{

     public class SimplexAlgo
     {
          //class variables
          private int numberofVariable;
          private int numberofConstraint;
          private int MinOrMax;
          private int[] varCondition;
          private double[][] inputTableau;
          private double[][] inputArray;
          private int varCounter;

          public int state; //state : optimal (1), unbound (2), multiple (3),negative(4), cycling (5),zrow no fullin(6)
          public List<DataTable> outputTableau = new List<DataTable>();
          public int tableauNumber;
          public List<string> enterVariable = new List<string>();
          public List<string> leaveVariable = new List<string>();
          public DataTable Solution = new DataTable("Solutions");
          public DataTable standardLP = new DataTable("Standard LP");
          public List<DataTable> mulSolution = new List<DataTable>();
          public List<List<DataTable>> mulList = new List<List<DataTable>>();
          public int cycledTableauNo;


        //class constructor
          #region Poh
          public SimplexAlgo(double[][] _inputArray, int _numberofVariable, int _numberofConstraint, int[] _varCondition, int _MinOrMax)
          {

               inputArray = _inputArray;
               numberofVariable = _numberofVariable;
               numberofConstraint = _numberofConstraint;
               varCondition = _varCondition;
               MinOrMax = _MinOrMax;
               convert();
          }

          //class methods

          //Convert input jagged array into standard LP (datatable)  and jagged array of first tableau 
          private void convert()
          {
               
              //check if objective is filled full or not
               int cCo = 0;
               for (int i = 0; i < numberofVariable; i++)
               {
                    if (inputArray[0][i] == 0)
                    {
                         cCo++;
                    }
               }
               if (cCo == numberofVariable)
               {
                    state = 6;
               }
               varCounter = 0;
               //counting number of variables after turning them positive(due to urs. /+/-)
               for (int i = 0; i < numberofVariable; i++)
               {
                    if (varCondition[i] == 1)
                    {
                         varCounter = varCounter + 2;
                    }
                    else if ((varCondition[i] == 2) || (varCondition[i] == 3))
                    {
                         varCounter = varCounter + 1;
                    }
               }


               double[][] tempTableau = new double[numberofConstraint + 1][];

               for (int i = 0; i <= numberofConstraint; i++)
               {
                    tempTableau[i] = new double[varCounter + numberofConstraint + 1];
               }
               int convertCount = 0;
               for (int i = 0; i < (numberofVariable); i++)
               {

                    if (varCondition[i] == 1)
                    {
                         tempTableau[0][convertCount] = -1 * inputArray[0][i];
                         tempTableau[0][convertCount + 1] = inputArray[0][i];
                         convertCount = convertCount + 2;
                    }
                    if (varCondition[i] == 2)
                    {
                         tempTableau[0][convertCount] = -1 * inputArray[0][i];
                         convertCount = convertCount + 1;
                    }
                    if (varCondition[i] == 3)
                    {
                         tempTableau[0][convertCount] = inputArray[0][i];
                         convertCount = convertCount + 1;
                    }

               }

               for (int i = varCounter; i <= (varCounter + numberofConstraint); i++)
               {
                    tempTableau[0][i] = 0;
               }

               convertCount = 0;
               for (int f = 1; f <= numberofConstraint; f++)
               {
                    for (int i = 0; i < (numberofVariable); i++)
                    {

                         if (varCondition[i] == 1)
                         {
                              tempTableau[f][convertCount] = inputArray[f][i];
                              tempTableau[f][convertCount + 1] = -1 * inputArray[f][i];
                              convertCount = convertCount + 2;
                         }
                         if (varCondition[i] == 2)
                         {
                              tempTableau[f][convertCount] = inputArray[f][i];
                              convertCount = convertCount + 1;
                         }
                         if (varCondition[i] == 3)
                         {
                              tempTableau[f][convertCount] = -1 * inputArray[f][i];
                              convertCount = convertCount + 1;
                         }

                    }
                    for (int i = varCounter; i <= (varCounter + numberofConstraint); i++)
                    {

                         if (i == (varCounter + numberofConstraint))
                         {
                              tempTableau[f][i] = inputArray[f][numberofVariable];
                         }
                         else
                         {
                              tempTableau[f][i] = 0;
                         }
                    }
                    convertCount = 0;
               }
               int slackCount = varCounter;
               for (int i = 1; i <= numberofConstraint; i++)
               {
                    if (tempTableau[i][varCounter + numberofConstraint] < 0)
                    {
                         state = 4;
                         for (int j = 0; j < varCounter; j++)
                         {
                              tempTableau[i][j] = -1 * tempTableau[i][j];
                         }
                         tempTableau[i][varCounter + numberofConstraint] = -1 * tempTableau[i][varCounter + numberofConstraint];
                         tempTableau[i][slackCount] = -1;
                         slackCount++;
                    }

                   
                    
                    else if (tempTableau[i][varCounter + numberofConstraint] >= 0)
                    {
                         tempTableau[i][slackCount] = 1;
                         slackCount++;
                    }

               }


               inputTableau = tempTableau;

              
               //Generate arrays of variables. 
               string[][] var = new string[2][];
              

               var[0] = new string[varCounter + numberofConstraint]; //first row variable
               var[1] = new string[numberofConstraint];//first column variable
               //Generation of Variables
               int arrayCount = 0;
               int xCount = 0;
               int sCount = 0;
               //first row x-variables
               for (int i = 0; i < numberofVariable; i++)
               {
                    if (varCondition[i] == 1)
                    {
                         xCount++;
                         var[0][arrayCount] = "x" + Convert.ToString(xCount) + "⁺";
                         var[0][arrayCount + 1] = "x" + Convert.ToString(xCount) + "⁻";
                         arrayCount = arrayCount + 2;
                    }
                    else if (varCondition[i] == 2)
                    {
                         xCount++;
                         var[0][arrayCount] = "x" + Convert.ToString(xCount);
                         arrayCount++;
                    }

                    else if (varCondition[i] == 3)
                    {
                         xCount++;
                         var[0][arrayCount] = "x" + Convert.ToString(xCount) + "⁻";
                         arrayCount++;
                    }
               }
               //first row s-variables
               for (int i = 0; i < numberofConstraint; i++)
               {

                    sCount = i + 1;
                    var[0][arrayCount] = "s" + Convert.ToString(sCount);
                    arrayCount++;
               }
               //first column s-variables
               sCount = 0;
               for (int i = 0; i < numberofConstraint; i++)
               {
                    sCount = i + 1;
                    var[1][i] = "s" + Convert.ToString(sCount);

               }
               //standard LP Table
               standardLP.Columns.Add("Standard Form", typeof(string));
               for (int i = 0; i < (varCounter + numberofConstraint); i++)
               {
                    standardLP.Columns.Add(var[0][i], typeof(double));
               }
               standardLP.Columns.Add("DELETE", typeof(string));
               standardLP.Columns.Add("RHS", typeof(double));
               for (int i = 0; i <= numberofConstraint; i++)
               {
                    DataRow row = standardLP.NewRow();

                    for (int j = 0; j <= (varCounter + numberofConstraint + 2); j++)
                    {

                         if ((i == 0) && (j == 0))
                         {
                              if (MinOrMax == 0)
                              {
                                   row[j] = "Minimize :";
                              }
                              else if (MinOrMax == 1)
                              {
                                   row[j] = "Maximize :";
                              }
                         }
                         if ((i == 1) && (j == 0))
                         {
                              row[j] = "Subjective to :";
                         }
                         if ((i == 0) && (j > 0) && (j <= varCounter))
                         {
                              row[j] = -1 * (Math.Round(inputTableau[i][j - 1], 4));
                         }
                         if ((i != 0) && (j != 0) && (j != (varCounter + numberofConstraint + 1)) && (j != (varCounter + numberofConstraint + 2)))
                         {
                              row[j] = Math.Round(inputTableau[i][j - 1], 4);
                         }
                         if ((i > 0) && (j == (varCounter + numberofConstraint + 1)))
                         {
                              row[j] = "=";
                         }
                         if ((i != 0) && (j == (varCounter + numberofConstraint + 2)))
                         {
                              row[j] = inputTableau[i][varCounter + numberofConstraint];
                         }
                    }

                    standardLP.Rows.Add(row);
               }
               //simplex
               if ((state != 4)&&(state != 6))
               {
                    simplex();
               }

          }


          //simplex method
          private void simplex()
          {
               int optimalFlag = 0;
               tableauNumber = 0;
               string leaveVar;
               string enterVar;

               ////////////////////////////////
               //Generate arrays of variables. 
               string[][] var = new string[2][];


               var[0] = new string[varCounter + numberofConstraint]; //first row variable
               var[1] = new string[numberofConstraint];//first column variable
               //Generation of Variables
               int arrayCount = 0;
               int xCount = 0;
               int sCount = 0;
               //first row x-variables
               for (int i = 0; i < numberofVariable; i++)
               {
                    if (varCondition[i] == 1)
                    {
                         xCount++;
                         var[0][arrayCount] = "x" + Convert.ToString(xCount) + "⁺";
                         var[0][arrayCount + 1] = "x" + Convert.ToString(xCount) + "⁻";
                         arrayCount = arrayCount + 2;
                    }
                    else if (varCondition[i] == 2)
                    {
                         xCount++;
                         var[0][arrayCount] = "x" + Convert.ToString(xCount);
                         arrayCount++;
                    }

                    else if (varCondition[i] == 3)
                    {
                         xCount++;
                         var[0][arrayCount] = "x" + Convert.ToString(xCount) + "⁻";
                         arrayCount++;
                    }
               }
               //first row s-variables
               for (int i = 0; i < numberofConstraint; i++)
               {

                    sCount = i + 1;
                    var[0][arrayCount] = "s" + Convert.ToString(sCount);
                    arrayCount++;
               }
               //first column s-variables
               sCount = 0;
               for (int i = 0; i < numberofConstraint; i++)
               {
                    sCount = i + 1;
                    var[1][i] = "s" + Convert.ToString(sCount);

               }

               ///start varComp- for multiple comparison
               string[][] varComp = new string[2][];
               varComp[0] = new string[varCounter + numberofConstraint]; //first row variable
               varComp[1] = new string[numberofConstraint];//first column variable
               //Generation of Variables
               arrayCount = 0;
               xCount = 0;
               sCount = 0;
               //first row x-variables
               for (int i = 0; i < numberofVariable; i++)
               {
                    if (varCondition[i] == 1)
                    {
                         xCount++;
                         varComp[0][arrayCount] = "x" + Convert.ToString(xCount);
                         varComp[0][arrayCount + 1] = "x" + Convert.ToString(xCount);
                         arrayCount = arrayCount + 2;
                    }
                    else if (varCondition[i] == 2)
                    {
                         xCount++;
                         varComp[0][arrayCount] = "x" + Convert.ToString(xCount);
                         arrayCount++;
                    }

                    else if (varCondition[i] == 3)
                    {
                         xCount++;
                         varComp[0][arrayCount] = "x" + Convert.ToString(xCount);
                         arrayCount++;
                    }
               }
               //first row s-variables
               for (int i = 0; i < numberofConstraint; i++)
               {

                    sCount = i + 1;
                    varComp[0][arrayCount] = "s" + Convert.ToString(sCount);
                    arrayCount++;
               }
               //first column s-variables
               sCount = 0;
               for (int i = 0; i < numberofConstraint; i++)
               {
                    sCount = i + 1;
                    varComp[1][i] = "s" + Convert.ToString(sCount);

               }
               ///end

               tableauNumber++;

               DataTable dt = new DataTable("Output Tableau");

               dt.Columns.Add("Basic", typeof(string));
               for (int i = 0; i < (varCounter + numberofConstraint); i++)
               {
                    dt.Columns.Add(var[0][i], typeof(double));
               }
               dt.Columns.Add("Solution", typeof(double));
               int dcount = 0;
               for (int i = 0; i <= numberofConstraint; i++)
               {
                    DataRow row = dt.NewRow();

                    for (int j = 0; j <= (varCounter + numberofConstraint + 1); j++)
                    {
                         if (j == 0)
                         {
                              if (i == 0)
                              {
                                   row[j] = "z";
                              }
                              else
                              {
                                   row[j] = var[1][dcount];
                                   dcount++;
                              }
                         }
                         else if (j != 0)
                         {
                              row[j] = Math.Round(inputTableau[i][j - 1], 4);
                         }
                    }

                    dt.Rows.Add(row);
               }

               outputTableau.Add(dt);



               //iteration loop, loop until optimal stage or facing special case, etc:unbound
               while (optimalFlag == 0)
               {
                    //check whether optimal stage is reached
                    //check whether all coefficients are nonpositive for minimization
                    if (MinOrMax == 0)
                    {
                         int count = 0;
                         for (int i = 0; i < (varCounter + numberofConstraint); i++)//not checking solution column
                         {
                              if (inputTableau[0][i] <= 0)
                              {
                                   count++;
                              }
                         }
                         if (count == (varCounter + numberofConstraint))
                         {
                              state = 1;
                              break;
                         }
                         else
                              optimalFlag = 0;

                    }
                    //check whether all coefficients are nonnegative for maximization
                    if (MinOrMax == 1)
                    {
                         int count = 0;
                         for (int i = 0; i < (varCounter + numberofConstraint); i++)//not checking solution column
                         {
                              if (inputTableau[0][i] >= 0)
                              {
                                   count++;
                              }
                         }
                         if (count == (varCounter + numberofConstraint))
                         {
                              state = 1;
                              break;
                         }
                         else
                              optimalFlag = 0;
                    }

                    int enterColumn = 0;
                    //find column of entering variable for maximization, most negative
                    if (MinOrMax == 1)
                    {

                         List<int> validColumn = new List<int>();
                         for (int i = 0; i < (varCounter + numberofConstraint); i++)
                         {
                              if (varComp[1].Contains(varComp[0][i]))
                              {

                              }
                              else
                              {
                                   validColumn.Add(i);
                              }
                         }

                         double mostNeg = inputTableau[0][validColumn[0]];
                         enterColumn = validColumn[0];
                         for (int i = 1; i < validColumn.Count; i++)
                         {
                              if (inputTableau[0][validColumn[i]] < mostNeg)
                              {
                                   mostNeg = inputTableau[0][validColumn[i]];
                                   enterColumn = validColumn[i];
                              }
                         }
                    }


                    //find column of entering variable for minimization, most positive
                    if (MinOrMax == 0)
                    {

                         List<int> validColumn = new List<int>();
                         for (int i = 0; i < (varCounter + numberofConstraint); i++)
                         {
                              if (varComp[1].Contains(varComp[0][i]))
                              {

                              }
                              else
                              {
                                   validColumn.Add(i);
                              }
                         }

                         double mostPos = inputTableau[0][validColumn[0]];
                         enterColumn = validColumn[0];
                         for (int i = 1; i < validColumn.Count; i++)
                         {
                              if (inputTableau[0][validColumn[i]] > mostPos)
                              {
                                   mostPos = inputTableau[0][validColumn[i]];
                                   enterColumn = validColumn[i];
                              }
                         }
                    }
                    

                    ////////////////////////////////////////////////////////////////////
                    //check if the current iteration is unbound or not which is
                    //when the coefficient for the denominator of the intercept ratios are either zero or negative
                    int unboundCounter = 0;
                    for (int i = 1; i <= numberofConstraint; i++)
                    {
                         if((inputTableau[i][enterColumn] == 0)||((inputTableau[i][varCounter + numberofConstraint]/ inputTableau[i][enterColumn])< 0))
                         {
                              unboundCounter++;
                         }
                    }


                    if (unboundCounter == numberofConstraint)
                    {

                         state = 2;
                         break;//if the problem is unbound, then break the loop
                    }

                    //find row of leaving variable, check the row with smallest ratio
                    int leaveRow = 1;
                    double smallestRatio = 100000000000000;// 

                    for (int i = 1; i <= numberofConstraint; i++)
                    {
                         if ((inputTableau[i][enterColumn] != 0) && (inputTableau[i][varCounter + numberofConstraint] / inputTableau[i][enterColumn] >= 0) && (smallestRatio > inputTableau[i][varCounter + numberofConstraint] / inputTableau[i][enterColumn]))
                         //if divide by 0->infinity, if negative-> unbound,numberofConstraint + numberofVariable= the solution's column
                         {
                              smallestRatio = inputTableau[i][varCounter + numberofConstraint] / inputTableau[i][enterColumn];
                              leaveRow = i;
                         }
                    }
                    //
                    enterVar = var[0][enterColumn];
                    leaveVar = var[1][leaveRow - 1];
                    enterVariable.Add(enterVar);
                    leaveVariable.Add(leaveVar);



                    //Swapping of variables: non basic to basic
                    var[1][leaveRow - 1] = var[0][enterColumn];
                    varComp[1][leaveRow - 1] = varComp[0][enterColumn];

                    //iteration
                    double pivot = inputTableau[leaveRow][enterColumn];
                    for (int i = 0; i <= (varCounter + numberofConstraint); i++)
                    {
                         inputTableau[leaveRow][i] = (inputTableau[leaveRow][i]) / pivot;
                    }
                    for (int i = 0; i <= numberofConstraint; i++)
                    {
                         if (i != leaveRow)
                         {
                              double entcolcoeff = inputTableau[i][enterColumn];
                              for (int j = 0; j <= (varCounter + numberofConstraint); j++)
                              {
                                   inputTableau[i][j] = inputTableau[i][j] - entcolcoeff * (inputTableau[leaveRow][j]);

                              }
                         }
                    }

                    dt = new DataTable("Output Tableau");
                    dt.Columns.Add("Basic", typeof(string));
                    for (int i = 0; i < (varCounter + numberofConstraint); i++)
                    {
                         dt.Columns.Add(var[0][i], typeof(double));
                    }
                    dt.Columns.Add("Solution", typeof(double));
                    dcount = 0;
                    for (int i = 0; i <= numberofConstraint; i++)
                    {
                         DataRow row = dt.NewRow();

                         for (int j = 0; j <= (varCounter + numberofConstraint + 1); j++)
                         {
                              if (j == 0)
                              {
                                   if (i == 0)
                                   {
                                        row[j] = "z";
                                   }
                                   else
                                   {
                                        row[j] = var[1][dcount];
                                        dcount++;
                                   }
                              }
                              else if (j != 0)
                              {
                                   row[j] = Math.Round(inputTableau[i][j - 1], 4);
                              }
                         }

                         dt.Rows.Add(row);

                    }
                    int cyclingFlag = 0;
                    for (int i = 0; i < outputTableau.Count; i++)
                    {
                         if (areTheTablesSame(dt, outputTableau[i]) == true)
                         {
                              cyclingFlag = 1;
                              cycledTableauNo = i;
                         }
                    }
                    if (cyclingFlag == 1)
                    {
                         outputTableau.Add(dt);
                         state = 5;
                         break;
                    }
                    outputTableau.Add(dt);

                    tableauNumber++;

                    //check whether optimal stage is reached
                    //check whether all coefficients are nonpositive for minimization
                    if (MinOrMax == 0)
                    {
                         int count = 0;
                         for (int i = 0; i < (varCounter + numberofConstraint); i++)//not checking solution column
                         {
                              if (inputTableau[0][i] <= 0)
                              {
                                   count++;
                              }
                         }
                         if (count == (varCounter + numberofConstraint))
                         {
                              state = 1;
                              break;
                         }
                         else
                              optimalFlag = 0;

                    }
                    //check whether all coefficients are nonnegative for maximization
                    if (MinOrMax == 1)
                    {
                         int count = 0;
                         for (int i = 0; i < (varCounter + numberofConstraint); i++)//not checking solution column
                         {
                              if (inputTableau[0][i] >= 0)
                              {
                                   count++;
                              }
                         }
                         if (count == (varCounter + numberofConstraint))
                         {
                              state = 1;
                              break;
                         }
                         else
                              optimalFlag = 0;
                    }


               }

               //double check
               if(state !=5)
               {
                    if (MinOrMax == 0)
                    {
                         int count = 0;
                         for (int i = 0; i < (varCounter + numberofConstraint); i++)//not checking solution column
                         {
                              if (inputTableau[0][i] <= 0)
                              {
                                   count++;
                              }
                         }
                         if (count == (varCounter + numberofConstraint))
                         {
                              state = 1;
                         }
                    }

                    //check whether all coefficients are nonnegative for maximization
                    if (MinOrMax == 1)
                    {
                         int count = 0;
                         for (int i = 0; i < (varCounter + numberofConstraint); i++)//not checking solution column
                         {
                              if (inputTableau[0][i] >= 0)
                              {
                                   count++;
                              }
                         }
                         if (count == (varCounter + numberofConstraint))
                         {
                              state = 1;
                         }
                    }
               }
               //
               //check if it is multiple solutions case,
               //When zero appears in the column of a nonbasic variable in the z-row
               int zeroCounter = 0;
               for (int i = 0; i < (varCounter + numberofConstraint); i++)
               {

                    if (inputTableau[0][i] == 0)
                    {
                         zeroCounter++;
                    }
               }
               //compare number of zeroes, if more , means at zrow nonbasic's has zero
               int multiFlag = 0;
               int multiCounter = 0;
               if (zeroCounter > numberofConstraint)
               {
                    for (int i = 0; i < (varCounter + numberofConstraint); i++)
                    {
                         if (inputTableau[0][i] == 0)//search for zero
                         {
                              for (int j = 0; j < numberofConstraint; j++)
                              {
                                   if (varComp[0][i] != varComp[1][j])//compare whether it is basic
                                   {
                                        multiCounter++;
                                   }
                              }

                              if (multiCounter == numberofConstraint)//got zero but not basic
                              {
                                   multiFlag = 1;
                                   break;
                              }
                              else if (multiFlag < numberofConstraint)
                              {
                                   multiCounter = 0;
                              }
                         }

                    }
               }

               if ((multiFlag == 1) && (state != 5)&& (state == 1)&&(state !=2))
               {
                    state = 3;
                    multiFlag = 0;
               }

               //solution
               ////////////////////////////////
               //Generate arrays of variables consisting of "+" & "-" for urs, +ve, -ve
               string[] solVar = new string[varCounter + numberofConstraint];
               arrayCount = 0;
               xCount = 0;
               sCount = 0;
               //first row x-variables
               for (int i = 0; i < numberofVariable; i++)
               {
                    if (varCondition[i] == 1)
                    {
                         xCount++;
                         solVar[arrayCount] = "x" + Convert.ToString(xCount) + "⁺";
                         solVar[arrayCount + 1] = "x" + Convert.ToString(xCount) + "⁻";
                         arrayCount = arrayCount + 2;
                    }
                    else if (varCondition[i] == 2)
                    {
                         xCount++;
                         solVar[arrayCount] = "x" + Convert.ToString(xCount);
                         arrayCount++;
                    }

                    else if (varCondition[i] == 3)
                    {
                         xCount++;
                         solVar[arrayCount] = "x" + Convert.ToString(xCount) + "⁻";
                         arrayCount++;
                    }

               }
               //first row s-variables
               for (int i = 0; i < numberofConstraint; i++)
               {

                    sCount = i + 1;
                    solVar[arrayCount] = "s" + Convert.ToString(sCount);
                    arrayCount++;
               }

               double[] sol = new double[varCounter + numberofConstraint];
               for (int i = 0; i < (varCounter + numberofConstraint); i++)
               {
                    sol[i] = 0;
               }

               for (int i = 0; i < (varCounter + numberofConstraint); i++)
               {
                    for (int j = 0; j < numberofConstraint; j++)
                    {
                         if (solVar[i] == var[1][j])
                         {
                              sol[i] = inputTableau[j + 1][varCounter + numberofConstraint];
                         }
                    }
               }
               //Generate original variables without considering +/-
               string[] solVariable = new string[numberofVariable + numberofConstraint];
               arrayCount = 0;
               xCount = 0;
               sCount = 0;
               //first row x-variables
               for (int i = 0; i < numberofVariable; i++)
               {
                    xCount++;
                    solVariable[arrayCount] = "x" + Convert.ToString(xCount);
                    arrayCount++;
               }
               //first row s-variables
               for (int i = 0; i < numberofConstraint; i++)
               {

                    sCount = i + 1;
                    solVariable[arrayCount] = "s" + Convert.ToString(sCount);
                    arrayCount++;
               }



               double[] solution = new double[numberofVariable + numberofConstraint];
               for (int i = 0; i < (numberofVariable + numberofConstraint); i++)
               {
                    solution[i] = 0;
               }
               int solCount = 0;

               for (int i = 0; i < (numberofVariable + numberofConstraint); i++)
               {
                    if (i < numberofVariable)
                    {
                         if (varCondition[i] == 1)
                         {
                              solution[i] = sol[solCount] - sol[solCount + 1];
                              solCount = solCount + 2;
                         }

                         else if (varCondition[i] == 2)
                         {
                              solution[i] = sol[solCount];
                              solCount++;
                         }

                         else if (varCondition[i] == 3)
                         {
                              solution[i] = -1 * sol[solCount];
                              solCount++;
                         }
                    }
                    else if (i >= numberofVariable)
                    {
                         solution[i] = sol[solCount];
                         solCount++;
                    }
               }

               //solution data table
               if ((state == 1) || (state == 3))
               {
                    Solution.Columns.Add("z", typeof(double));
                    for (int i = 0; i < (numberofVariable + numberofConstraint); i++)
                    {
                         Solution.Columns.Add(solVariable[i], typeof(double));
                    }


                    DataRow row = Solution.NewRow();

                    for (int j = 0; j <= (numberofVariable + numberofConstraint); j++)
                    {

                         if (j == 0)
                         {
                              row[j] = Math.Round(inputTableau[0][varCounter + numberofConstraint], 4);
                         }
                         else if (j > 0)
                         {
                              row[j] = Math.Round(solution[j - 1], 4);
                         }

                    }
                    Solution.Rows.Add(row);
               }
               if (state == 3)
               {
                    multiple(var, varComp);
               }

               if((mulList.Count == 0)&&(mulSolution.Count == 0)&&(state==3))
               {
                    state = 1;
               }
          }

          //Perform simplex to find every alternate solutions. 
          private void multiple(string [][] _var, string [][] _varComp)
          {
               List<DataTable> altoutputTableau = new List<DataTable>();

               DataTable altSolution = new DataTable();

               int optimalFlag = 0;
               tableauNumber = 0;
               string leaveVar;
               string enterVar;

               string[][] var = new string[2][];


               var[0] = new string[varCounter + numberofConstraint]; //first row variable
               var[1] = new string[numberofConstraint];//first column variable
               var[0] = _var[0];
               var[1] = _var[1];

               string[][] varComp = new string[2][];

               varComp[0] = new string[varCounter + numberofConstraint]; //first row variable
               varComp[1] = new string[numberofConstraint];//first column variable
               varComp[0] = _varComp[0];
               varComp[1] = _varComp[1];

               //iteration loop, loop until optimal stage or facing special case, etc:unbound
               while (optimalFlag == 0)
               {
                    int enterColumn = 0;
                    //find column of entering variable for maximization, most negative
                    if (MinOrMax == 1)
                    {

                         List<int> validColumn = new List<int>();
                         for (int i = 0; i < (varCounter + numberofConstraint); i++)
                         {
                              if (varComp[1].Contains(varComp[0][i]))
                              {

                              }
                              else
                              {
                                   validColumn.Add(i);
                              }
                         }
                         
                         double mostNeg = inputTableau[0][validColumn[0]];
                         enterColumn = validColumn[0];
                         for (int i = 1; i < validColumn.Count; i++)
                         {
                              if (inputTableau[0][validColumn[i]] < mostNeg)
                              {
                                   mostNeg = inputTableau[0][validColumn[i]];
                                   enterColumn = validColumn[i];
                              }
                         }
                    }


                    //find column of entering variable for minimization, most positive
                    if (MinOrMax == 0)
                    {

                         List<int> validColumn = new List<int>();
                         for (int i = 0; i < (varCounter + numberofConstraint); i++)
                         {
                              if (varComp[1].Contains(varComp[0][i]))
                              {

                              }
                              else
                              {
                                   validColumn.Add(i);
                              }
                         }

                         double mostPos = inputTableau[0][validColumn[0]];
                         enterColumn = validColumn[0];
                         for (int i = 1; i < validColumn.Count; i++)
                         {
                              if (inputTableau[0][validColumn[i]] > mostPos)
                              {
                                   mostPos = inputTableau[0][validColumn[i]];
                                   enterColumn = validColumn[i];
                              }
                         }
                    }

                    ////////////////////////////////////////////////////////////////////
                    //check if the current iteration is unbound or not which is
                    //when the coefficient for the denominator of the intercept ratios are either zero or negative
                    int unboundCounter = 0;
                    for (int i = 1; i <= numberofConstraint; i++)
                    {
                         if ((inputTableau[i][enterColumn] == 0) || ((inputTableau[i][varCounter + numberofConstraint] / inputTableau[i][enterColumn]) < 0))
                         {
                              unboundCounter++;
                         }
                    }

                    if (unboundCounter == numberofConstraint)
                    {

                         state = 2;
                         break;//if the problem is unbound, then break the loop
                    }

                    //find row of leaving variable, check the row with smallest ratio
                    int leaveRow = 1;
                    double smallestRatio = 100000000000000;// 

                    for (int i = 1; i <= numberofConstraint; i++)
                    {
                         if ((inputTableau[i][enterColumn] != 0) && (inputTableau[i][varCounter + numberofConstraint] / inputTableau[i][enterColumn] >= 0) && (smallestRatio > inputTableau[i][varCounter + numberofConstraint] / inputTableau[i][enterColumn]))
                         //if divide by 0->infinity, if negative-> unbound,numberofConstraint + numberofVariable= the solution's column
                         {
                              smallestRatio = inputTableau[i][varCounter + numberofConstraint] / inputTableau[i][enterColumn];
                              leaveRow = i;
                         }
                    }
                    //swapping of variables.
                    enterVar = var[0][enterColumn];
                    leaveVar = var[1][leaveRow - 1];

                    DataTable dt = new DataTable("Output Tableau");
                    dt.Columns.Add("Basic", typeof(string));
                    for (int i = 0; i < (varCounter + numberofConstraint); i++)
                    {
                         dt.Columns.Add(var[0][i], typeof(double));
                    }
                    dt.Columns.Add("Solution", typeof(double));
                    dt.Columns.Add("enter", typeof(string));
                    dt.Columns.Add("leave", typeof(string));


                    
                    for (int i = 0; i <= numberofConstraint; i++)
                    {
                         DataRow row = dt.NewRow();

                         for (int j = 0; j <= (varCounter + numberofConstraint + 3); j++)
                         {
                              if((i==0)&&(j==0))
                              {
                                   row[j] = "z";
                              }
                              else if((j==0)&&(i > 0))
                              {
                                   row[j] = var[1][i - 1];
                              }
                              else if ((j != 0)&&(j <= (varCounter + numberofConstraint + 1)))
                              {
                                   row[j] = Math.Round(inputTableau[i][j - 1], 4);
                              }
                              else if ((i == 0)&&(j == (varCounter + numberofConstraint + 2)))
                              {
                                   row[j] = enterVar;
                              }
                              else if ((i == 0) && (j == (varCounter + numberofConstraint + 3)))
                              {
                                   row[j] = leaveVar;
                              }
                         }

                         dt.Rows.Add(row);

                    }

                    int cyclingFlag = 0;
                    for (int i = 0; i < outputTableau.Count; i++)
                    {
                         if (areTheTablesSame(dt, outputTableau[i]) == true)
                         {
                              cyclingFlag = 1;
                              cycledTableauNo = i;
                         }
                    }
                    if (cyclingFlag == 1)
                    {
                         //altoutputTableau.Add(dt);
                         state = 5;
                         break;
                    }
                    if (altoutputTableau.Count > 0)
                    {
                         for (int i = 0; i < altoutputTableau.Count; i++)
                         {
                              if (areTheTablesSame(dt, altoutputTableau[i]) == true)
                              {
                                   cyclingFlag = 1;
                                   cycledTableauNo = i;
                              }
                         }
                         if (cyclingFlag == 1)
                         {
                             // altoutputTableau.Add(dt);
                              state = 5;
                              break;
                         }
                    }
                   

                    //add to local list of data tables
                    altoutputTableau.Add(dt);


                    //Swapping of variables: non basic to basic
                    var[1][leaveRow - 1] = var[0][enterColumn];
                    varComp[1][leaveRow - 1] = varComp[0][enterColumn];

                    //iteration
                    double pivot = inputTableau[leaveRow][enterColumn];
                    for (int i = 0; i <= (varCounter + numberofConstraint); i++)
                    {
                         inputTableau[leaveRow][i] = (inputTableau[leaveRow][i]) / pivot;
                    }
                    for (int i = 0; i <= numberofConstraint; i++)
                    {
                         if (i != leaveRow)
                         {
                              double entcolcoeff = inputTableau[i][enterColumn];
                              for (int j = 0; j <= (varCounter + numberofConstraint); j++)
                              {
                                   inputTableau[i][j] = inputTableau[i][j] - entcolcoeff * (inputTableau[leaveRow][j]);

                              }
                         }
                    }

                    //check whether optimal stage is reached
                    //check whether all coefficients are nonpositive for minimization
                    if (MinOrMax == 0)
                    {
                         int count = 0;
                         for (int i = 0; i < (varCounter + numberofConstraint); i++)//not checking solution column
                         {
                              if (inputTableau[0][i] <= 0)
                              {
                                   count++;
                              }
                         }
                         if (count == (varCounter + numberofConstraint))
                         {
                              state = 1;
                              break;
                         }
                         else
                              optimalFlag = 0;

                    }
                    //check whether all coefficients are nonnegative for maximization
                    if (MinOrMax == 1)
                    {
                         int count = 0;
                         for (int i = 0; i < (varCounter + numberofConstraint); i++)//not checking solution column
                         {
                              if (inputTableau[0][i] >= 0)
                              {
                                   count++;
                              }
                         }
                         if (count == (varCounter + numberofConstraint))
                         {
                              state = 1;
                              break;
                         }
                         else
                              optimalFlag = 0;
                    }


               }
               //double check
               if (state != 5)
               {
                    if (MinOrMax == 0)
                    {
                         int count = 0;
                         for (int i = 0; i < (varCounter + numberofConstraint); i++)//not checking solution column
                         {
                              if (inputTableau[0][i] <= 0)
                              {
                                   count++;
                              }
                         }
                         if (count == (varCounter + numberofConstraint))
                         {
                              state = 1;
                         }
                    }

                    //check whether all coefficients are nonnegative for maximization
                    if (MinOrMax == 1)
                    {
                         int count = 0;
                         for (int i = 0; i < (varCounter + numberofConstraint); i++)//not checking solution column
                         {
                              if (inputTableau[0][i] >= 0)
                              {
                                   count++;
                              }
                         }
                         if (count == (varCounter + numberofConstraint))
                         {
                              state = 1;
                         }
                    }
               }
               //final tableau
               DataTable final = new DataTable("Output Tableau");
               final.Columns.Add("Basic", typeof(string));
               for (int i = 0; i < (varCounter + numberofConstraint); i++)
               {
                    final.Columns.Add(var[0][i], typeof(double));
               }
               final.Columns.Add("Solution", typeof(double));
               final.Columns.Add("enter", typeof(string));
               final.Columns.Add("leave", typeof(string));

             
               for (int i = 0; i <= numberofConstraint; i++)
               {
                    DataRow row = final.NewRow();

                    for (int j = 0; j <= (varCounter + numberofConstraint + 3); j++)
                    {
                         if ((i == 0) && (j == 0))
                         {
                              row[j] = "z";
                         }
                         else if ((j == 0) && (i > 0))
                         {
                              row[j] = var[1][i - 1];
                         }
                         else if ((j != 0) && (j <= (varCounter + numberofConstraint + 1)))
                         {
                              row[j] = Math.Round(inputTableau[i][j - 1], 4);
                         }
                         else if ((i == 0) && (j == (varCounter + numberofConstraint + 2)))
                         {
                              row[j] = "";
                         }
                         else if ((i == 0) && (j == (varCounter + numberofConstraint + 3)))
                         {
                              row[j] = "";
                         }
                    }

                    final.Rows.Add(row);

               }
               //add to local list of data tables
               altoutputTableau.Add(final);

               
               //check if it is multiple solutions case,
               //When zero appears in the column of a nonbasic variable in the z-row
               int zeroCounter = 0;
               for (int i = 0; i < (varCounter + numberofConstraint); i++)
               {

                    if (inputTableau[0][i] == 0)
                    {
                         zeroCounter++;
                    }
               }

               //compare number of zeroes, if more , means at zrow nonbasic's has zero
               int multiFlag = 0;
               int multiCounter = 0;
               if (zeroCounter > numberofConstraint)
               {
                    for (int i = 0; i < (varCounter + numberofConstraint); i++)
                    {
                         if (inputTableau[0][i] == 0)//search for zero
                         {
                              for (int j = 0; j < numberofConstraint; j++)
                              {
                                   if (varComp[0][i] != varComp[1][j])//compare whether it is basic
                                   {
                                        multiCounter++;
                                   }
                              }

                              if (multiCounter == numberofConstraint)//got zero but not basic
                              {
                                   multiFlag = 1;
                                   break;
                              }
                              else if (multiFlag < numberofConstraint)
                              {
                                   multiCounter = 0;
                              }
                         }

                    }
               }


               if ((multiFlag == 1) && (state != 5) && (state == 1) && (state != 2))
               {
                    state = 3;
                    multiFlag = 0;
               }
               //solution
               ////////////////////////////////
               //Generate arrays of variables consisting of "+" & "-" for urs, +ve, -ve
               string[] solVar = new string[varCounter + numberofConstraint];
               int arrayCount = 0;
               int xCount = 0;
               int sCount = 0;
               //first row x-variables
               for (int i = 0; i < numberofVariable; i++)
               {
                    if (varCondition[i] == 1)
                    {
                         xCount++;
                         solVar[arrayCount] = "x" + Convert.ToString(xCount) + "⁺";
                         solVar[arrayCount + 1] = "x" + Convert.ToString(xCount) + "⁻";
                         arrayCount = arrayCount + 2;
                    }
                    else if (varCondition[i] == 2)
                    {
                         xCount++;
                         solVar[arrayCount] = "x" + Convert.ToString(xCount);
                         arrayCount++;
                    }

                    else if (varCondition[i] == 3)
                    {
                         xCount++;
                         solVar[arrayCount] = "x" + Convert.ToString(xCount) + "⁻";
                         arrayCount++;
                    }

               }
               //first row s-variables
               for (int i = 0; i < numberofConstraint; i++)
               {

                    sCount = i + 1;
                    solVar[arrayCount] = "s" + Convert.ToString(sCount);
                    arrayCount++;
               }

               double[] sol = new double[varCounter + numberofConstraint];
               for (int i = 0; i < (varCounter + numberofConstraint); i++)
               {
                    sol[i] = 0;
               }

               for (int i = 0; i < (varCounter + numberofConstraint); i++)
               {
                    for (int j = 0; j < numberofConstraint; j++)
                    {
                         if (solVar[i] == var[1][j])
                         {
                              sol[i] = inputTableau[j + 1][varCounter + numberofConstraint];
                         }
                    }
               }
               //Generate original variables without considering +/-
               string[] solVariable = new string[numberofVariable + numberofConstraint];
               arrayCount = 0;
               xCount = 0;
               sCount = 0;
               //first row x-variables
               for (int i = 0; i < numberofVariable; i++)
               {
                    xCount++;
                    solVariable[arrayCount] = "x" + Convert.ToString(xCount);
                    arrayCount++;
               }
               //first row s-variables
               for (int i = 0; i < numberofConstraint; i++)
               {

                    sCount = i + 1;
                    solVariable[arrayCount] = "s" + Convert.ToString(sCount);
                    arrayCount++;
               }



               double[] solution = new double[numberofVariable + numberofConstraint];
               for (int i = 0; i < (numberofVariable + numberofConstraint); i++)
               {
                    solution[i] = 0;
               }
               int solCount = 0;

               for (int i = 0; i < (numberofVariable + numberofConstraint); i++)
               {
                    if (i < numberofVariable)
                    {
                         if (varCondition[i] == 1)
                         {
                              solution[i] = sol[solCount] - sol[solCount + 1];
                              solCount = solCount + 2;
                         }

                         else if (varCondition[i] == 2)
                         {
                              solution[i] = sol[solCount];
                              solCount++;
                         }

                         else if (varCondition[i] == 3)
                         {
                              solution[i] = -1 * sol[solCount];
                              solCount++;
                         }
                    }
                    else if (i >= numberofVariable)
                    {
                         solution[i] = sol[solCount];
                         solCount++;
                    }
               }

               //solution data table
               if ((state == 1) || (state == 3))
               {
                    altSolution.Columns.Add("z", typeof(double));
                    for (int i = 0; i < (numberofVariable + numberofConstraint); i++)
                    {
                         altSolution.Columns.Add(solVariable[i], typeof(double));
                    }


                    DataRow row = altSolution.NewRow();

                    for (int j = 0; j <= (numberofVariable + numberofConstraint); j++)
                    {

                         if (j == 0)
                         {
                              row[j] = Math.Round(inputTableau[0][varCounter + numberofConstraint], 4);
                         }
                         else if (j > 0)
                         {
                              row[j] = Math.Round(solution[j - 1], 4);
                         }

                    }
                    altSolution.Rows.Add(row);
               }

               int compareflag = 0;
               if (areTheTablesSame(altSolution, Solution) == true)
               {
                    compareflag = 1;
               }
               for (int i = 0; i <mulSolution.Count;i++)
               {
                    if (areTheTablesSame(altSolution, mulSolution[i]) == true)
                    {
                         compareflag = 1;
                    }
                    
               }

               if (compareflag != 1)
               {
                    mulList.Add(altoutputTableau);
                    mulSolution.Add(altSolution);
                    multiple(var, varComp);
                    compareflag = 0;
               }
             
          }
        #endregion

        #region YL
        private bool areTheTablesSame(DataTable dtInput1, DataTable dtInput2)
        {
            DataTable dt1 = dtInput1.Copy();
            DataTable dt2 = dtInput2.Copy();
            bool same = true;

            try
            {
                if (dt1.Columns.Count > 0)
                {
                    string defaultSort = dt1.Columns[0].ColumnName + " asc"; //sort the rows based on first column name; not able to handle all situations but sufficient for current usage
                    DataView dv1 = dt1.DefaultView;
                    dv1.Sort = defaultSort;
                    dt1 = dv1.ToTable();

                    DataView dv2 = dt2.DefaultView;
                    dv2.Sort = defaultSort;
                    dt2 = dv2.ToTable();

                    //obtain all columnNames, except the first one
                    string[] columnNames = (from dc in dt1.Columns.Cast<DataColumn>()
                                            where dc.ColumnName.ToString() != dt1.Columns[0].ColumnName
                                            select dc.ColumnName).ToArray();

                    //compare every cells based on column name
                    for (int x = 0; x < dt1.Rows.Count; x++)
                    {
                        for (int y = 0; y < columnNames.Length; y++)
                        {
                            try
                            {
                                if (dt1.Rows[x][columnNames[y]].ToString() != dt2.Rows[x][columnNames[y]].ToString())
                                {
                                    return false;
                                }
                            }
                            catch (Exception ex)
                            {
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    same = false;
                }
                return same;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion
    }
}