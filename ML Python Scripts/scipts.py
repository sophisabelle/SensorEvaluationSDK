
#########script 1 - get multiple sensors in one row

import pandas as pd

def azureml_main(dataframe1 = None, dataframe2 = None):

    # Execution logic goes here
    print('Input pandas.DataFrame #1:\r\n\r\n{0}'.format(dataframe1))

    # get data by timestamp (sensors in one row)
    
    # get unique sensor names
    sensorNames = dataframe1.sensorName.unique()

    # get first common rows as dataframe
    
    sessioninfo = dataframe1.loc[dataframe1['sensorName'] == sensorNames[0]]
    sessioninfo = sessioninfo[['timeStamp', 'sessionID', 'patientID', 'startTime', 'numberOfSensors']]
    sessioninfo = sessioninfo.reset_index(drop=True)
    
    dict = {}
    dict['session'] = sessioninfo
    
    # go through connected sensors
    for name in sensorNames:
        sensordat = dataframe1.loc[dataframe1['sensorName'] == name]
        
        # get relevant rows
        sensordat = sensordat[['sensorPosition','accX','accY','accZ','gyroX','gyroY','gyroZ']]
        # get locaction
        loc = sensordat['sensorPosition'].iloc[0]
        #only keep dataval
        sensordat2 = sensordat[['accX','accY','accZ','gyroX','gyroY','gyroZ']]
        #rename columns according to location
        sensordat2 = sensordat2.add_suffix(loc)
        #reset the index
        sensordat2 = sensordat2.reset_index(drop=True)
        # add to dict
        dict[name] = sensordat2
    
    # concat all dataframe
    newdat = pd.concat(dict, axis=1)
    newdat.columns = newdat.columns.get_level_values(1)
    # get rid of null rows
    newdat = newdat.dropna()
    
    # Return value must be of a sequence of pandas.DataFrame
    return newdat,

######## script 2 - get summary geatures per 10 data rows and create new dataset

import pandas as pd

def azureml_main(dataframe1 = None, dataframe2 = None):

    # Execution logic goes here
    
    # make new dataframe with colums

    # set rows per segment
    num = 10
    # look at 10 rows at a time
    length = dataframe1.shape[0]
    segments = (length - length % num) / num
    
    start = 0
    end = num
    timeinterval = 0
    
    finalset = {}
    
    for x in range(segments):
        collect = {}
        selected = dataframe1[start:end]
        start += num
        end +=num
    
        info = selected[['sessionID']][0:1]
        info['timeInterval'] = timeinterval
        timeinterval+=1
        info = info.reset_index(drop=True)
        collect['info'] = info
        
        # get descriptive information
        desc = selected.describe()
        
        for index in desc.index.values:
            indexval = index.replace("%", "")
            rowdf = desc.loc[[index]]
            rowdf = rowdf.add_suffix(indexval)
            rowdf= rowdf.reset_index(drop=True)
            collect[indexval] = rowdf
            
            
        summarystat = pd.concat(collect, axis=1)
        summarystat.columns = summarystat.columns.get_level_values(1)
        finalset[x] = summarystat
    
    #concate all individual rows into a dataset
    finaldf = pd.concat(finalset, axis=0, ignore_index=True)
    #finaldf.columns = finaldf.columns.get_level_values(1)
    
    # Return value must be of a sequence of pandas.DataFrame
    return finaldf,


    ###### script 3 ######

import pandas as pd


def azureml_main(dataframe1 = None, dataframe2 = None):

    # Execution logic goes here
    print('Input pandas.DataFrame #1:\r\n\r\n{0}'.format(dataframe1))

    dataframe1 = dataframe1[~dataframe1.sensorName.duplicated()]

    
    # Return value must be of a sequence of pandas.DataFrame
    return dataframe1


    #### script 4 ####

import pandas as pd
from datetime import datetime

def azureml_main(dataframe1 = None, dataframe2 = None):

    # Execution logic goes here
    print('Input pandas.DataFrame #1:\r\n\r\n{0}'.format(dataframe1))
    
    sessionStart = dataframe1['timeStamp'].min()
    sessionEnd = dataframe1['timeStamp'].max()
    sessionId = dataframe1['sessionID'].iloc[0]
    
    t0 = datetime(1, 1, 1)
    now = datetime.utcnow()
    seconds = (now - t0).total_seconds() + 3600
    ticks = seconds * 10**7
    dateTimeUploaded = ticks
    
    d = {'SessionID': [sessionId], 'SessionStart': [sessionStart], 'SessionEnd': [sessionEnd], 'DateTimeUploaded': [dateTimeUploaded]}
    df = pd.DataFrame(data=d)

    return df,


####script 5 - get session summary analysis (add timeintervals for each movement)

import pandas as pd

def azureml_main(dataframe1 = None, dataframe2 = None):

    # Execution logic goes here
    print('Input pandas.DataFrame #1:\r\n\r\n{0}'.format(dataframe1))
    
    
    newdat = dataframe1.groupby('Scored Labels').count()
    newdat = newdat.reset_index()
    newdat.loc[:,'timeInterval'] *= 0.875
    
    running = 0
    walking = 0
    limping = 0
    standing = 0
    walking = 0
    sitting = 0
    lying = 0
    
    for index, row in newdat.iterrows():
        if row['Scored Labels'] == 'Running':
            running += row['timeInterval']
        elif row['Scored Labels'] == 'Walking':
            walking += row['timeInterval']
        elif row['Scored Labels'] == 'Limping':
            limping += row['timeInterval']
        elif row['Scored Labels'] == 'Standing':
            standing += row['timeInterval']
        elif row['Scored Labels'] == 'Sitting':
            sitting += row['timeInterval']
        elif row['Scored Labels'] == 'Lying down':
            lying += row['timeInterval']
    
    f = {'sessionID': [dataframe1['sessionID'][0]], 'AnalysisTimeRunning': [running], 'AnalysisTimeWalking': [walking], 'AnalysisTimeLimping': [limping], 'AnalysisTimeStanding': [standing], 'AnalysisTimeSitting': [sitting], 'AnalysisTimeLying': [lying], 'AnalysisTimeActive': [running + walking + limping], 'AnalysisTotalTime': [running + walking + limping + standing + sitting + lying]}
    
    dataframe3 = pd.DataFrame(data=f) 
    
    dataframe2 = dataframe2[~dataframe2.sessionID.duplicated()]
    
    dataframe4 = dataframe3.join(dataframe2.set_index('sessionID'), on='sessionID')

    return dataframe4


#### script 6 - join two datasets


import pandas as pd

def azureml_main(dataframe1 = None, dataframe2 = None):

    # Execution logic goes here
    print('Input pandas.DataFrame #1:\r\n\r\n{0}'.format(dataframe1))
    dataframe3 = dataframe1.join(dataframe2.set_index('SessionID'), on='sessionID')
    
    return dataframe3,







