Print 'Loading Table: bradley_data';

update dbo.patients
set          first_name = '   ' + first_name + '   '
             ,middle_name = '   ' + middle_name + '   '
             ,last_name = '   ' + last_name + '   '
             ,name_suffix = '   ' + 'MD' + '   '
where first_name='Lillian'
and last_name='Infobutton'

Update dbo.patients
set          middle_name = 'A.'
where first_name='Chester'
and last_name='Arthur'