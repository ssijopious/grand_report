
from cmath import nan
from pickle import TRUE
from xml.etree.ElementTree import tostring
import pandas as pd
import statsmodels.api as sm
from decimal import Decimal as D
import itertools
import math
import numpy as np
#import matplotlib.pyplot as plt

#from sklearn import linear_model
#from sklearn.model_selection import train_test_split
#import csv 
#from pandas.api.types import is_numeric_dtype


# with statsmodels
def freg(dfInput, infVariable, trgtVariable):
   x = sm.add_constant(dfInput[infVariable]) # adding a constant
   model = sm.OLS(dfInput[trgtVariable], x).fit()
   predictions = model.predict(x) 
   fprint_reg(model.params, (x.drop(['const'], axis=1)).keys(), trgtVariable) 
   #fig, ax = plt.subplots()
   #fig = sm.graphics.plot_fit(model.fit, 0, ax=ax)
   #ax.set_ylabel(trgtVariable)
   #ax.set_xlabel(infVariable)
   return model.params['const']   

def freg_cat(dfInput, dtColumn, trgtVariable):
   dummies = pd.concat((dfInput,pd.get_dummies(dfInput[dtColumn])), axis=1)
   y = dummies[trgtVariable]
   x = sm.add_constant(dummies.drop(trgtVariable+[dtColumn], axis=1))
   model = sm.OLS(y, x).fit()
   fprint_reg(model.params, (x.drop(['const'], axis=1)).keys(), trgtVariable)

# Print Results
def fprint_reg(model, infVariable, trgtVariable):
    print(trgtVariable[0] + ' (Const) : ' + str(D(model['const'])))    
    for variableFields in infVariable :
        print(variableFields + ' : ' + str(D(model[variableFields])))
    print("\n")

# Reg Adjust
def fregAdj (dfInput, infVariable, trgtVariable):
    for dtColumn in infVariable :
        x = sm.add_constant(dfInput[dtColumn]) # adding a constant
        model = sm.OLS(dfInput[trgtVariable], x).fit()
        predictions = model.predict(x) 
        print('***************************************')
        fprint_reg(model.params, (x.drop(['const'], axis=1)).keys(), trgtVariable) 
    return model.params['const']   

# Grouping
def fstratify(dfInput, infVariable, trgtVariable, stifyVariable):
    
     df=dfInput.sort_values(by = stifyVariable, ascending=True)
     dfGroup=[]
     for i in range(5):
         print('Group ' + str(i))
         dfGroup.append(df.iloc[i*int(len(df)/5):(i+1)*int(len(df)/5)])
         
         freg(dfGroup[i],infVariable,trgtVariable) 
         dfGroup[i].shape
         #print(dfGroup[i].describe()) # to see the nature of the dataset (mean, min, max, count, etc)
         i+=1

# Main function
def main():
    mstrfle = pd.ExcelFile("C:/Users/ssijo/Desktop/AQI_Test.xls") # excel file path tests
    excelMaster = pd.read_excel(mstrfle, 'MasterSheet') # reading the master sheet
    dfMaster=pd.DataFrame(excelMaster, columns= ['sheetName','targetVariable','variableFields','stratifyVariable']) # dataframe creation for master sheet 
    dfMaster = dfMaster.fillna({'stratifyVariable': ''}) # removing null
    dfMaster = dfMaster.reset_index()
    
    try:
        for index,dfRows in dfMaster.iterrows(): 
            excelReader = pd.read_excel(mstrfle, dfRows['sheetName']) #  reading data sheet 
            infVariable=list(dfRows['variableFields'].split(",")) # influencing variables
            trgtVariable=list(dfRows['targetVariable'].split(",")) # target variable
            dfInput= pd.DataFrame(excelReader, columns= (infVariable + trgtVariable)) 

            print('Influencing variables : ' + dfRows['variableFields'])
            print('Target variables : ' + dfRows['targetVariable'])
            print('Grouping variables : ' + dfRows['stratifyVariable'])
           
            # Categorical adjustment
            if 'object' in list(dfInput.dtypes) :    # categorical checking  
                for dtColumn in dfInput :
                    if dfInput[dtColumn].dtypes == 'O' and dtColumn in infVariable:       
                        print('Cat')
                        freg_cat(dfInput, dtColumn, trgtVariable) #Categorical reg. using dummyfing
                        print('********************************************************')     
                        dfavg=(dfInput.groupby(dtColumn)[trgtVariable].mean()).sort_values(trgtVariable) 
                        # Avg of target column by influvencing column and sort according to it (hot encoding numbering with groupby)
                        dfavg['row_num'] = np.arange(len(dfavg)) # 
                        dfavg.reset_index(inplace=True) # Removing the hierarchical index of dataframe                        
                        dfInput[dtColumn+"_cat"] =dfInput[dtColumn]  
                        print() 
                        for index, dtavgColumn in dfavg.iterrows() :
                            dfInput = dfInput.replace({dtColumn: {dtavgColumn[dtColumn] : dtavgColumn['row_num']}}) 
                            #Assigning values to object columns     
                        #dtColumn_cat= 
                           
           
            freg(dfInput,infVariable,trgtVariable)    # regression  ret =         *********
            fregAdj(dfInput,infVariable,trgtVariable)    # regression Adjust  ret =         *********
            #print("Return : " +str(ret))
            
            if (dfRows['stratifyVariable']!=''): # grouping
                fstratify(dfInput, infVariable, trgtVariable, dfRows['stratifyVariable'])  # stratification and regression 
                
                
    
    except NameError:
        print(NameError)

main()




def comment():
    #print(result)    

    # Stratification
    #print(dfInf[list(dfRows['variableFields'].split(","))])
    #if (dfRows['stratifyVariable']!=''):
    #    train, test = train_test_split(dfInf, test_size=0.2) 
    #    print(test)

        #stratify=xx[["X", "Y"]]

        #print(df.groupby(dfRows['stratifyVariable']).mean())

#print(df)
            #print(df.groupby(dfRows['stratifyVariable']).mean())


       #for i in range(len(dfGroup)) :
       #         x = sm.add_constant(dfGroup[i][list(dfRows['variableFields'].split(","))])
       #         print(dfGroup[i][list(dfRows['variableFields'].split(","))])




 #print_model = model.summary()
        #print(print_model)
        #print(model.params)
        #rslt=model.params
        #print(dfRows['targetVariable'] + ' (Const) : ' + str(D(model.params['const'])))
        #for variableFields in dfRows['variableFields'].split(",") :
        #    print(variableFields + ' : ' + str(D(model.params[variableFields])))


        #dfInf[dtColumn+"_cat"] =(dfInf[dtColumn].astype('category')).cat.codes  # assign the encoded variable
        #                print(dfInf)

        #print(dfavg.columns.values.tolist())

        #for i in range(len(dfavg)) : 
        #     dfInput = dfInput.replace(dfInput[dtColumn][dfInput.first_set[dfInput.first_set == �66�]],dtRows['row_num'])
        #     print()

        #replace_values = {'Medium' : 2, 'High' : 3 }

        print()