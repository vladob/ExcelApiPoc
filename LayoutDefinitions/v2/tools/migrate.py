"""Rebuild the isolated v2 package from preserved review sources; never edits v1."""
import csv,hashlib,io,json,re,zipfile
from pathlib import Path
R=Path(__file__).resolve().parents[1];V='2.0.0-draft.4'
Z=zipfile.ZipFile(R/'sources/IfoSoft-v2-review.zip')
def old(path):return json.loads(Z.read('IfoSoft-v2-review/'+path))
def sha(b):return hashlib.sha256(b).hexdigest()
def write(path,d):
 p=R/path;p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8',newline='\n');return p
def reference(id):return {'id':id,'version':V}
def envelope(id,kind,source=[]):return {'$schema':f'../schemas/{kind}.schema.json','schemaVersion':2,'definitionVersion':V,'id':id,'kind':kind,'status':'reviewDraft','productionApproved':False,'requiresCapabilities':['positioned.v2'],'sourceRefs':source,'reviewNotes':[]}
def rule(id,op,enabled=True,evidence='sourceDerived',**args):return dict(id=id,op=op,enabled=enabled,evidenceStatus=evidence,sourceRefs=[],reviewNotes=[],**args)
def sel(classes=[],members=[],level='any',occurrences='records'):return dict(semanticClasses=classes,aggregationMemberships=members,level=level,occurrences=occurrences)
def scope(kind='document',group=None):return dict(kind=kind,groupRef=group)
def match(text,mode='contains',ws='collapse'):return dict(mode=mode,value=text,ignoreCase=True,whitespace=ws,unicodeNormalization='NFC',regexTimeoutMs=100)
def pages(selection='all',maximum=10000):return dict(selection=selection,indices=[],maximumPages=maximum)
def loc():return dict(kind='normalizedPage',pages=pages(),assembly='readingOrder',cardinality='many',regionFraction=[0,0,1,1])
def pred(block):return dict(op='blockKind',blockRefs=[block])
def state(field,*states):return dict(op='fieldState',fieldRef=field,states=list(states))
def source(x):return {k:v for k,v in x.items() if k in ['file','line','sha256','rawCommand','expression','notes']}
def sources(x):return [source(x['source'])] if x.get('source') else []
def fieldop(n):return dict(kind='field',semanticName=n)
def calcop(n):return dict(kind='calculation',ruleRef=n)
# Parsers retain identifier fidelity and distinguish blank from numeric zero.
p=envelope('ifosoft.parsers','parsers');p['profiles']=[]
for id,x in old('common/parsers.v2.json')['profiles'].items():
 op=x['op']
 if op=='text':a=dict(trim=True,normalizeNbsp=True)
 elif op=='identifier':a=dict(trimOuterWhitespace=True,unicodeNormalization='NFC',preserveLeadingZeros=True,preserveInternalSpaces=True,allowLettersAndDiacritics=True)
 elif op=='integer':a=dict(allowGrouping=False,allowTrailingDot=True)
 elif op=='exactDate':a=dict(formats=x['formats'],twoDigitYear='uniqueExplicitContextOrAmbiguous',repairInvalidDate=False)
 elif op=='exactTime':a=dict(formats=x['formats'])
 elif op=='accountingPeriod':a=dict(pattern=x['pattern'],codeGroup='code',yearGroup='year',observedCodes=[f'{i:02}' for i in range(15)],unknownCode='retainWithDiagnostic',calendarDateConversion=False)
 else:a=dict(formats=[dict(decimalSeparator=f['decimal'],groupingSeparators=f['grouping'],wholeAmountSuffix=f['wholeAmountSuffix']) for f in x['formats']],groupingSize=3,oneGroupingConventionPerValue=True,allowLeadingSign=True,allowSpaceAfterSign=True,fractionDigits=2,blankState='blank',distinctSuccessfulValues='ambiguous')
 p['profiles'].append(rule(id,op,**a))
write('parsers/ifosoft.json',p)
# GMX account-plan authoring conversion only; all other point values copied verbatim.
def account_variant(filename,id):
 path=R/'sources'/filename;text=path.read_text(encoding='cp1250');f=72/254;page=[round(2100*f,6),round(2970*f,6)];blocks=[];origin=0;b=None
 names={'SYN':'syntheticComponent','ANA':'analyticalComponent','NAZOV':'accountName','TYP':'accountType','P':'POD','D':'DAN','S':'STR','Z':'ODD','K':'POL','SALDO':'SAL','DPH':'DPH','$RECNO':'ordinal','$USER':'licensedUserText','$PAGENO':'printedPageNumber','$C1':'printedCount','$C2':'printedCount','$C3':'printedCount','SYN%1%1':'accountClassCode','SYN%2':'accountGroupCode'}
 for no,line in enumerate(text.splitlines(),1):
  m=re.match(r'#CPART\([^,]+,([^,]+),',line)
  if m:origin=float(m[1])
  m=re.match(r'#(PAGEHEAD|RECORD|FOOT[123])\((\d+),',line)
  if m:
   bid=m[1].lower();b=dict(id=bid,sourceDirective=line,originSourceY=origin,frame='nominalPage' if bid=='pagehead' else 'localBlock',repeat={'nominalAdvancePt':round(int(m[2])*f,6)},fields=[],labels=[],lines=[]);blocks.append(b)
  m=re.match(r'\|(CPDATABASE|CPLABEL|CPLINE)\((.*)\)',line)
  if not m or b is None:continue
  args=next(csv.reader([m[2]]));x,y,x2,y2=map(float,args[:4]);norm=lambda a,c:[round(a*f,6),round((2970-c if b['id']=='pagehead' else origin-c)*f,6)]
  s=dict(file=filename,line=no,rawCommand=line)
  if m[1]=='CPLINE':b['lines'].append(dict(id=f'line-{no}',pointsPt=[norm(x,y),norm(x2,y2)],source=s));continue
  r=[norm(x,y2)[0],norm(x,y2)[1],norm(x2,y)[0],norm(x2,y)[1]]
  if m[1]=='CPLABEL':b['labels'].append(dict(id=f'label-{no}',text=args[-1],rectPt=r,source=s));continue
  expr=next(a.strip("'") for a in args if a.startswith("'"));name=names.get(expr,'source_'+expr);parser='integer' if name in ['ordinal','printedCount','printedPageNumber'] else 'text' if name in ['accountName','licensedUserText'] else 'code'
  b['fields'].append(dict(id=f'field-{no}',name=name,parser=parser,rectPt=r,source=s,sourceExpression=expr))
 return dict(id=id,nominalPageSizePt=page,source=dict(file=filename,sha256=sha(path.read_bytes())),blocks=blocks)
layouts=[];originals={};family_layouts={}
for path in sorted(n for n in Z.namelist() if '/layouts/' in n):
 d=json.loads(Z.read(path));originals[d['id']]=d;short=Path(path).name.replace('.v2.json','');category='unresolved' if 'obr-ucet' in short else d['category']
 l=envelope(d['id'],'layout',[dict(file='sources/IfoSoft-v2-review.zip',sha256=sha((R/'sources/IfoSoft-v2-review.zip').read_bytes()))]);l.update(formats=d['formats'],modelRef=reference('ifosoft.model.'+short),identificationRefs=[reference(d['identificationRef'])],parserRefs=[reference(p['id'])],validationRefs=[reference(d['validationRef'])],variants=[],metadata=[],metadataDerivations=[],classification=[],classificationPolicy=dict(evaluation='allRules',noMatch='unrecognizedCandidate',incompatibleMatches='ambiguous'),groups=[],completeness=[],openQuestions=d.get('openQuestions',[]))
 if category=='unresolved':l['openQuestions'].append('OBR_UCET account movements retained; AJ/GL domain mapping is unresolved.')
 vs=d['variants'] if category!='AF' else [account_variant('_UROZVRH.gmx','urozvrh'),account_variant('_UROZVRH_V2.gmx','urozvrh-v2-unverified')]
 if category=='AF':l['openQuestions']=['GMX replaces prior empirical geometry. UROZVRH_V2 is unverified against print samples.','Count membership, blank numbered rows and heading rows require full-file reconciliation.','Reporting entity CIN/name and fiscal year have no verified printed locators; licensed user is not reporting entity.']
 for v in vs:
  nv=dict(id=v['id'],evidenceStatus='unverified' if 'unverified' in v['id'] else 'sourceDerived',sourceRefs=sources(v),nominalPageSizePt=v['nominalPageSizePt'],anchors=[],blocks=[])
  for b in v['blocks']:
   bid=b['id'];frame='nominalPage' if b.get('frame')=='nominalPage' else 'blockLocal';kind='pageHeader' if bid=='pagehead' else 'repeatingRecord' if bid=='record' else 'reportFooter' if bid=='foot1' else 'decoration' if bid=='pagefoot' else 'groupFooter'
   nb=dict(id=bid,kind=kind,frame=frame,sourceRefs=[dict(file=v['source']['file'],rawCommand=b['sourceDirective'])],nominalAdvancePt=b.get('repeat',{}).get('nominalAdvancePt'),emptyTemplate=not any(b.get(k) for k in ['fields','labels','lines']),placement=rule('locate-'+bid,'locateBlock',frame=frame,origin='pageCalibration' if frame=='nominalPage' else 'independentOccurrence',signals=['rulings','labels','fieldColumns','unassignedText'],combination='union',onAmbiguous='retainHypotheses'),candidateDetection=rule('detect-'+bid,'detectCandidates',signals=['rulings','labels','fieldColumns','unassignedText'],combination='union',requiresSuccessfulParsing=False),fieldAssignment=rule('assign-'+bid,'assignFields',paddingPt=1.5,assembly='glyphClusters',splitAcrossFields='requireGlyphGeometry',overlap='retainAmbiguity',retainClippedSource=True),fields=[],labels=[],lines=[])
   for x in b['fields']:
    ss=sources(x)
    if x.get('sourceExpression'):ss[0]['expression']=x['sourceExpression']
    nb['fields'].append(dict(id=x['id'],semanticName=x['name'],rectPt=x['rectPt'],parserRef=x['parser'],sourceRefs=ss))
   for x in b.get('labels',[]):nb['labels'].append(dict(id=x['id'],text=x['text'],rectPt=x['rectPt'],sourceRefs=sources(x)))
   for x in b.get('lines',[]):nb['lines'].append(dict(id=x['id'],pointsPt=x['pointsPt'],sourceRefs=sources(x)))
   if nb['emptyTemplate']:
    for k in ['placement','candidateDetection','fieldAssignment']:nb[k]['enabled']=False
   nv['blocks'].append(nb)
  header=next(b for b in nv['blocks'] if b['id']=='pagehead')
  # Preserve source anchors exactly, add qualified ruling references.
  anchors=v.get('calibration',{}).get('anchors')
  if anchors:
   for a in anchors:
    line=next(x for x in header['lines'] if x['id']==a['sourceLineRef']);idx=line['pointsPt'].index(a['pointPt'])
    nv['anchors'].append(dict(id=a['id'],feature='rulingEndpoint',pointPt=a['pointPt'],blockRef='pagehead',lineRef=line['id'],endpointIndex=idx,evidenceStatus='sourceDerived',sourceRefs=line['sourceRefs']))
  else:
   for line in header['lines']:
    for idx,point in enumerate(line['pointsPt']):nv['anchors'].append(dict(id=f'{line["id"]}-{idx}',feature='rulingEndpoint',pointPt=point,blockRef='pagehead',lineRef=line['id'],endpointIndex=idx,evidenceStatus=nv['evidenceStatus'],sourceRefs=line['sourceRefs']))
  nv['calibration']=rule('page-calibration','fitUniformScaleTranslation',evidence='unverified',anchorRefs=[a['id'] for a in nv['anchors']],minimumInliers=3,minimumAnchorSeparationPt=150,minimumSpanPt=[150,10],scaleRange=[0.65,1.4],maximumResidualPt=1.5,maximumHypotheses=256,onFailure='retainUnresolvedPage',inferScaleFromPageSize=False)
  nv['calibration']['reviewNotes']=['Provisional thresholds retained from draft 2; require corpus tuning.']
  l['variants'].append(nv)
  metadata_names={'entityName':'reportingEntity.name','licensedUserText':'licensedUserText','periodStart':'period.start','periodEnd':'period.end','printingDate':'printingDate','printingTime':'printingTime','printedPageNumber':'printedPageLabel'}
  for b in nv['blocks']:
   for x in b['fields']:
    if x['semanticName'] not in metadata_names:continue
    l['metadata'].append(dict(id=f'{nv["id"]}.{b["id"]}.{x["id"]}',semanticName=metadata_names[x['semanticName']],locator=dict(kind='blockField',variantRef=nv['id'],blockRef=b['id'],fieldRef=x['id'],frame=b['frame'],pages=pages(),assembly='glyphClusters',cardinality='zeroOrOne'),parserRef=x['parserRef'],onAbsent='unresolved',onConflict='ambiguous',sourceRefs=x['sourceRefs']))
 if any(m['semanticName']=='period.start' for m in l['metadata']) and any(m['semanticName']=='period.end' for m in l['metadata']):
  l['metadataDerivations']=[rule('fiscal-year','deriveFiscalYear',inputMetadataNames=['period.start','period.end'],outputMetadataName='fiscalYear',onDifferentYears='unresolved',onMissingInput='unresolved',origin='derived')]
 # Titles are observed text in the calibrated source label region.
 for v in l['variants']:
  head=next(b for b in v['blocks'] if b['id']=='pagehead')
  for label in head['labels']:
   compact=''.join(label['text'].upper().split())
   if any(t in compact for t in ['DENNÍK','HLAVNÁKNIHA','ÚČTOVÝROZVRH','ÚČTOVNÉHOROZVRHU','OBRATY']):
    l['metadata'].append(dict(id=v['id']+'.title.'+label['id'],semanticName='reportTitle',locator=dict(kind='calibratedRegion',variantRef=v['id'],frame='nominalPage',rectPt=label['rectPt'],paddingPt=2,pages=pages(),assembly='glyphClusters',cardinality='zeroOrOne'),parserRef='text',onAbsent='unresolved',onConflict='ambiguous',sourceRefs=label['sourceRefs']))
 l['openQuestions'].append('No verified reportingEntity.cin locator in the supplied GMX fields. Return unresolved; do not extract arbitrary eight-digit numbers or use manifest expectations.')
 # Semantic classifications use detected block occurrences, never business ID length.
 if category!='AF':
  for c in d['classification']['rules']:
   level=c.get('level','detail' if c['export'] else 'any');membership=['exportedDetails'] if c['export'] else ['syntheticSummaries'] if c.get('level')=='synthetic' else []
   l['classification'].append(rule(c['id'],'classify',when=pred(c['block']),semanticClass=c['semanticType'],exportEligibility='review' if category=='unresolved' and c['export'] else 'eligible' if c['export'] else 'excluded',aggregationMemberships=membership,level=level))
 else:
  for bid,cl in [('pagehead','pageHeader'),('foot1','reportControl'),('foot2','classCount'),('foot3','groupCount')]:l['classification'].append(rule(cl,'classify',when=pred(bid),semanticClass=cl,exportEligibility='excluded',aggregationMemberships=[],level='any'))
  heading=dict(op='textMatch',fieldRef='analyticalComponent',match=match('****','exact'))
  for id,conditions,cl,export in [('heading',[heading],'accountGroupHeading','excluded'),('account',[state('syntheticComponent','present'),{'op':'not','condition':heading}],'accountDefinition','eligible'),('numbered-blank',[state('ordinal','present'),state('syntheticComponent','blank'),state('analyticalComponent','blank')],'numberedBlankRow','review')]:
   l['classification'].append(rule(id,'classify',when=dict(op='all',conditions=[pred('record')]+conditions),semanticClass=cl,exportEligibility=export,aggregationMemberships=['numberedRows'],level='detail'))
 # Group boundaries explicitly identify closure and preserve incomplete input.
 gs=d['groups']['definitions'] if category!='AF' else [dict(id='accountGroup',parent='accountClass',closingBlock='foot3',keyField='accountGroupCode'),dict(id='accountClass',parent='reportSection',closingBlock='foot2',keyField='accountClassCode')]
 l['groups'].append(dict(id='reportSection',parentRef=None,memberSelector=sel(),startRule={'op':'sectionStart'},endRule={'op':'sectionEnd'},keyFields=[],crossPagePolicy='requireContinuityEvidence',partialInputPolicy='retainIncomplete',repeatedHeaderResets=False,inheritance=[]))
 for g in gs:
  members=['numberedRows'] if category=='AF' else ['syntheticSummaries'] if g['id']=='accountClass' and any(c['aggregationMemberships']==['syntheticSummaries'] for c in l['classification']) else ['exportedDetails']
  l['groups'].append(dict(id=g['id'],parentRef=g['parent'],memberSelector=sel(members=members),startRule={'op':'afterPreviousClosure'},endRule=dict(op='match',when=pred(g['closingBlock'])),keyFields=g.get('keyFields',[g['keyField']] if 'keyField'in g else []),crossPagePolicy='requireContinuityEvidence',partialInputPolicy='retainIncomplete',repeatedHeaderResets=False,inheritance=[dict(fieldRef=n,from_='closingOccurrence',when='absent',onConflict='retainBothAndDiagnose') for n in g.get('inherit',{}).get('fields',[])]))
  for h in l['groups'][-1]['inheritance']:h['from']=h.pop('from_')
 validation=old('validation/'+short+'.v2.json')
 for c in validation['rules']:
  if c['op'] in ['requireFields','requireAny']:l['completeness'].append(rule(c['id'],c['op'],selector=sel([c['selectType']]),fieldRefs=c['fields'],requiredState='present'))
 l['completeness'] += [rule('all-candidates','accountForCandidates',onUnrecognized='preventCleanSuccess'),rule('all-tokens','accountForTokens',decorationRequiresRuleId=True),rule('coverage','inspectCoverage',printedPageField='printedPageNumber',contiguousPagesProveOriginalComplete=False),rule('spacing','checkSpacing',evidence='unverified',excludeExplainedBlocks=True,maximumAdvanceMultiplier=1.5,onGap='suspectedMissingRecord')]
 if category=='AF':l['completeness'].append(rule('ordinal','sequence',fieldRef='ordinal',scope=scope(),increment=1,onGap='diagnosticOnly'))
 # One model per layout keeps domain mapping deliberate and prevents accidental reuse.
 m=envelope('ifosoft.model.'+short,'model');fields={}
 types={'text':'text','code':'identifier','date':'date','time':'time','period':'accountingPeriod','integer':'integer','money-comma':'decimal','money-dot':'decimal'}
 for v in l['variants']:
  for b in v['blocks']:
   for x in b['fields']:fields[x['semanticName']]=dict(id=x['semanticName'],type=types[x['parserRef']],domainTarget=None)
 m.update(category=category,fields=list(fields.values()),domainMappingStatus='pendingReview');m['reviewNotes']=['C# domain export mapping is deferred to Step 3; source fields are preserved.'];write('models/'+short+'.json',m)
 # Concrete control bindings, no prose-only executable checks.
 val=envelope(d['validationRef'],'validation');val.update(layoutRef=reference(l['id']),printedControls=[],calculations=[],rules=[])
 for v in l['variants']:
  c_by_block={c['when']['blockRefs'][0]:c for c in l['classification'] if c['when']['op']=='blockKind'}
  for b in v['blocks']:
   if b['id'] not in c_by_block or b['id'] in ['pagehead','pagefoot','record']:continue
   for x in b['fields']:
    if not x['parserRef'].startswith('money') and x['semanticName']!='printedCount':continue
    cid=f'{v["id"]}.{b["id"]}.{x["id"]}';gid=next((g['id'] for g in l['groups'] if g['endRule'].get('when',{}).get('blockRefs')==[b['id']]),None)
    sc=scope('group',gid) if gid else scope();member=sel(members=['numberedRows' if category=='AF' else 'exportedDetails'])
    val['printedControls'].append(dict(id=cid,variantRef=v['id'],blockRef=b['id'],fieldRef=x['id'],scope=sc))
    args=dict(scope=sc,memberSelector=member,inputRefs=[] if category=='AF' else [fieldop(x['semanticName'])],prerequisites=['resolvedClassification','resolvedGroupMembership','unambiguousOperands','completeScope'],onMissingInput='inconclusive')
    if category=='AF':args.update(mode='occurrences',distinctKey=[])
    val['calculations'].append(rule('calc.'+cid,'count' if category=='AF' else 'sum',enabled=False,evidence='unverified',**args))
    r=rule('compare.'+cid,'compare',enabled=False,evidence='unverified',scope=sc,calculatedRef='calc.'+cid,printedControlRef=cid,absoluteTolerance='0' if category=='AF' else '0.01',coverageRequirement='completeScope',onMissingInput='inconclusive',onPartialCoverage='notEvaluated');r['reviewNotes']=['Disabled pending fixture reconciliation of membership, scope and printed control semantics.'];val['rules'].append(r)
 # Row arithmetic is explicit and never substitutes zero for blank operands.
 if category=='GL':
  sc=scope('record');selection=sel(['accountBalance']);pre=['resolvedClassification','unambiguousOperands']
  def calculation(id,op,inputs):
   val['calculations'].append(rule(id,op,scope=sc,memberSelector=selection,inputRefs=inputs,prerequisites=pre,onMissingInput='inconclusive'))
  if 'openingNet' in fields:
   calculation('balance.opening-plus-debit','add',[fieldop('openingNet'),fieldop('turnoverDebit')])
   calculation('balance.expected','subtract',[calcop('balance.opening-plus-debit'),fieldop('turnoverCredit')])
   # Add explicit zero as arithmetic identity, not a replacement for absent input.
   calculation('balance.printed','add',[fieldop('closingNet'),dict(kind='literal',decimal='0')])
  else:
   calculation('balance.opening-net','subtract',[fieldop('openingDebit'),fieldop('openingCredit')])
   calculation('balance.opening-plus-debit','add',[calcop('balance.opening-net'),fieldop('turnoverDebit')])
   calculation('balance.expected','subtract',[calcop('balance.opening-plus-debit'),fieldop('turnoverCredit')])
   calculation('balance.printed','subtract',[fieldop('closingDebit'),fieldop('closingCredit')])
  val['rules'].append(rule('balance-equation','compare',scope=sc,calculatedRef='balance.expected',otherCalculatedRef='balance.printed',absoluteTolerance='0.01',coverageRequirement='completeScope',onMissingInput='inconclusive',onPartialCoverage='notEvaluated'))
 val['reviewNotes']=['Blank monetary values remain blank; no implicit effective zero.','CSV expected fiscal year is comparison-only and never supplies parsing or detection context.','Printed aggregate controls remain disabled pending fixture reconciliation.']

 write('validation/'+short+'.json',val);write('layouts/'+short+'.json',l);layouts.append(l);family_layouts.setdefault(d['identificationRef'],[]).append(l)
# Independent producer/category/layout scores. Generic titles never prove producer.
for id,ls in family_layouts.items():
 family=id.rsplit('.',1)[-1];o=old('identification/'+family+'.v2.json');n=envelope(id,'identification');n.update(scan=dict(initialPages=pages('first',3),additionalPages=pages('all',20),expandWhen='unresolvedDecision',onBudgetExhausted='retainCoverageAndUnresolvedDecisions'),signals=[],evidenceGroups=[dict(id='title',scoreCap=35,repeatedOccurrences='maximumOnce'),dict(id='columns',scoreCap=25,repeatedOccurrences='maximumOnce'),dict(id='producer',scoreCap=40,repeatedOccurrences='maximumOnce'),dict(id='geometry',scoreCap=50,repeatedOccurrences='maximumOnce')],decisions=[],layoutCandidates=[reference(l['id']) for l in ls],onTie='retainCandidates',scoreMeaning='rankingNotProbability')
 cat='unresolved' if family=='obr_ucet' else o['category']
 for sig in o['signals']:
  if sig['id']=='marker':continue
  args=dict(locator=loc(),target='category',candidate=cat,weight=sig['weight'],evidenceGroup='columns' if sig['op']=='orderedLabels' else 'title',polarity='support',missing='neutral')
  if sig['op']=='orderedLabels':args.update(labels=[match(x) for x in sig['labels']],axis='x',maximumOrthogonalGapPt=40);op='orderedLabels'
  else:args['match']=match(sig['text'],ws='remove');op='textMatch'
  n['signals'].append(rule(sig['id'],op,**args))
 if family=='uctrozvrh':
  n['signals'].append(rule('title-v2','textMatch',locator=loc(),target='category',candidate='AF',weight=35,evidenceGroup='title',polarity='support',missing='neutral',match=match('Opis účtovného rozvrhu analytických účtov',ws='remove')))
 n['signals'].append(rule('producer-brand','textMatch',locator=loc(),target='producer',candidate='IfoSoft',weight=40,evidenceGroup='producer',polarity='support',missing='neutral',match=match('IFOsoft')))
 marker=next((s for s in o['signals'] if s['id']=='marker'),None)
 if marker:n['signals'].append(rule('producer-marker','markerMatch',locator=loc(),target='producer',candidate='IfoSoft',weight=40,evidenceGroup='producer',polarity='support',missing='neutral',match=match(marker['text'],ws='remove')))
 def decision(target,candidate,minimum,required=[],gates=[]):return dict(target=target,candidate=candidate,minimumScore=minimum,minimumLead=15,requiredSignals=required,vetoSignals=[],requiredDecisions=gates)
 n['decisions'] += [decision('category',cat,50),decision('producer','IfoSoft',40)]
 for index,l in enumerate(ls):
  sid='geometry-'+str(index);n['signals'].append(rule(sid,'geometryFit',evidence='unverified',locator=loc(),target='layout',candidate=l['id'],weight=50,evidenceGroup='geometry',polarity='support',missing='neutral',layoutRef=reference(l['id']),variantRefs=[v['id'] for v in l['variants']]))
  n['decisions'].append(decision('layout',l['id'],50,[sid],[dict(target='producer',candidate='IfoSoft'),dict(target='category',candidate=cat)]))
 n['reviewNotes']=['Scores and thresholds are provisional, not probabilities. Missing markers are neutral. Geometry is required for layout selection.','Scan budgets count physical available pages; test expectations and filenames are never recognition inputs.'];write('identification/'+family+'.json',n)
cat=envelope('ifosoft.catalog','catalog');cat['$schema']='schemas/catalog.schema.json';cat.update(definitions=[],geometry=dict(unit='pt',origin='lowerLeft',frame='uprightCropLocal',rectangleOrder=['left','bottom','right','top']),limits=dict(fileBytes=268435456,pageCount=10000,decompressedPackageBytes=1073741824,regexTimeoutMs=100,predicateDepth=32,candidateHypotheses=256,calibrationHypotheses=256,executionTimeMs=120000))
for folder in ['layouts','identification','models','parsers','validation']:
 for path in sorted((R/folder).glob('*.json')):
  d=json.loads(path.read_text(encoding='utf-8'));cat['definitions'].append(dict(ref=reference(d['id']),kind=d['kind'],path=str(path.relative_to(R)).replace('\\','/'),sha256=sha(path.read_bytes().replace(b'\r\n', b'\n'))))
write('catalog.json',cat)
print(f'Migrated {len(layouts)} layouts, {sum(len(l["variants"]) for l in layouts)} variants and {len(cat["definitions"])} definitions')
