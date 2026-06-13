print 'Loading Table: prn_indications';

begin transaction;

/*************************************
        begin loading permanent tables
*************************************/

set identity_insert [dbo].[prn_indications] on;

insert into [dbo].[prn_indications]
(
    [id]
  , [site_id]
  , [option_description]
)
select
    [val].[prn_id]
  , [val].[site_id]
  , [val].[option_description]
from (
values
  ('1', '0', 'bloating')
, ('2', '0', 'pain')
, ('3', '0', 'fever greater than 100.3 degrees F')
, ('4', '0', 'shortness of breath')
, ('5', '0', 'anxiety')
, ('6', '0', 'indigestion')
, ('7', '0', 'constipation')
, ('8', '0', 'allergies')
, ('9', '0', 'muscle spasms')
, ('10', '0', 'respiratory depression')
, ('11', '0', 'insomnia')
, ('12', '0', 'diarrhea')
, ('13', '0', 'nausea or vomiting')
, ('14', '0', 'cough')
, ('15', '0', 'itching')
, ('16', '0', 'chest pain')
, ('17', '0', 'agitation')
, ('18', '0', 'pain: severe (7 to 10)')
, ('19', '0', 'elevated blood pressure')
, ('20', '0', 'bradycardia')
, ('21', '0', 'seizure')
, ('22', '0', 'withdrawal')
, ('23', '0', 'pain: mild (1 to 3)')
, ('24', '0', 'pain: moderate (4 to 6)')
, ('25', '0', 'congestion')
, ('26', '0', 'cramps')
, ('27', '0', 'dizziness')
, ('28', '0', 'dysuria')
, ('29', '0', 'headache')
, ('30', '0', 'heart burn')
, ('31', '0', 'hiccups')
, ('32', '0', 'hypoglycemia')
, ('33', '0', 'hypotension')
, ('34', '0', 'sorethroat')
, ('35', '0', 'spasms')
, ('36', '0', 'tremors')
, ('37', '0', 'vertigo')
) as [val]
(
[prn_id]
, [site_id]
, [option_description]
)
    left join [dbo].[prn_indications] [prn_indication]
        on [prn_indication].[id] = [val].[prn_id]
where [prn_indication].[id] is null;

set identity_insert [dbo].[prn_indications] off;

/****************
        end table
****************/

commit transaction;