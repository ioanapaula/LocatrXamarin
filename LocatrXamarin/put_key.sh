#!/bin/bash
value=`cat maps_key.txt`
echo $value
sed -i "" "s/{Your_Api_Key}/$value/g" Resources/values/google_maps_api.xml